import { defineStore } from 'pinia'
import { Notify } from 'quasar'
import type { IOffsetsWithHit, IRoomPlayerDto } from 'src/api/rooms.api'
import {
  EHitType,
  ERoomState,
  fireInRoomAPI,
  getRoomAsOpponentByGuidAPI,
  leaveRoomAPI,
} from 'src/api/rooms.api'
import { useSignalR } from 'src/hooks/use-signalr'
import { useUserStore } from './user-store'
import { useShootingStarStore } from './shooting-star-store'
import type { IShipPlacement } from './room-placement-store'
import { convertShipToShipPlacement } from 'src/utils/misc'

export const GRID_SIZE = 8 + 1

interface IRoomUserSetup {
  ships: IShipPlacement[]
  firedOffsets: IOffsetsWithHit[]
}
interface IRoomOpponentSetup {
  firedOffsets: IOffsetsWithHit[]
}

export const useRoomFightStore = defineStore('room-fight', {
  state: () => ({
    hub: useSignalR('rooms-on'),
    userStore: useUserStore(),
    room: <IRoomPlayerDto | null>null,
    roomUserSetup: <IRoomUserSetup>{
      ships: [],
      firedOffsets: [],
    },
    roomOpponentSetup: <IRoomOpponentSetup>{
      firedOffsets: [],
    },
    playerOneReady: <boolean>false,
    playerTwoReady: <boolean>false,
    roomPlacingTimer: <number>0,
    roomLapTimer: <number>0,
    hasBeenCanceled: <boolean>false,
    lap: <number>1,
    winnerId: <number | null>null,
    playerWon: <boolean>false,
  }),
  getters: {
    isRoomPlaying(): boolean {
      if (this.room === null) return false
      return this.room.state === ERoomState.playing
    },
    isRoomPlacing(): boolean {
      if (this.room === null) return false
      return this.room.state === ERoomState.placing
    },
    isRoomPending(): boolean {
      if (this.room === null) return false
      return this.room.state === ERoomState.pending
    },
    isCurrentUserTurn(): boolean {
      if (this.room === null) return false
      if (this.room.state !== ERoomState.playing) return false
      if (this.playerWon) return false
      const lapOdd = this.lap % 2 === 0
      if (this.isCurrentUserPlayerOne) return !lapOdd
      return lapOdd
    },
    isCurrentUserPlayerOne(): boolean {
      if (this.room === null || this.userStore.user === null) return false
      return this.userStore.user.id === this.room.playerOne.id
    },
    shouldTriggerAlertWhenLeaving(): boolean {
      return (
        this.room !== null &&
        (this.room.state === ERoomState.placing || this.room.state === ERoomState.playing)
      )
    },
    getWinnerString(): string | null {
      if (this.room === null) return null
      if (this.hasBeenCanceled) return 'Partie annulée'
      let pseudo: string | null = null
      if (this.winnerId === this.room.playerOne.id) pseudo = this.room.playerOne.pseudo
      if (this.winnerId === this.room.playerTwo?.id) pseudo = this.room.playerTwo.pseudo
      if (pseudo == this.userStore.user!.pseudo) {
        return 'Vous avez gagner !'
      }
      return 'Vous avez perdu !'
    },
    getOpponentPseudo(): string {
      if (this.room === null) return ''
      if (this.isCurrentUserPlayerOne) return this.room.playerTwo?.pseudo ?? ''
      return this.room.playerOne.pseudo
    },
    getUserPseudo(): string {
      if (this.room === null) return ''
      if (this.isCurrentUserPlayerOne) return this.room.playerOne.pseudo
      return this.room.playerTwo?.pseudo ?? ''
    },
  },
  actions: {
    async register(roomGuid: string) {
      this.$reset()
      await this.setupRoom(roomGuid)
      this.hub.connection.on('OpponentJoined', (room: IRoomPlayerDto, playerId: number) => {
        this.room = room
        if (playerId !== this.userStore.user!.id) {
          Notify.create({
            color: 'positive',
            message: 'Un adversaire à rejoint la partie',
          })
        }
        if (this.room.state === ERoomState.placing) {
          Notify.create({
            color: 'negative',
            message: 'Placer vos vaisseaux !',
          })
        }
      })
      this.hub.connection.on('PlayerReady', (playerId: number) => {
        if (this.room === null || this.room.playerTwo === null) return
        let opponentPseudo: string = ''
        if (this.room.playerOne.id === playerId) {
          this.playerOneReady = true
          opponentPseudo = this.room.playerOne.pseudo
        }
        if (this.room.playerTwo.id === playerId) {
          this.playerTwoReady = true
          opponentPseudo = this.room.playerTwo.pseudo
        }
        if (this.userStore.user?.id === playerId) return
        Notify.create({
          color: 'positive',
          message: `${opponentPseudo} est prêt !`,
        })
      })
      this.hub.connection.on('GameOn', () => {
        if (this.room === null) return
        this.room.state = ERoomState.playing
        Notify.create({
          color: 'positive',
          message: 'La bataille commence !',
        })
      })
      this.hub.connection.on(
        'Move',
        (playerId: number, xOffset: number, yOffset: number, hit: EHitType) => {
          if (this.room === null || this.userStore.user === null || this.room.playerTwo === null)
            return
          if (playerId === this.userStore.user.id) return
            this.roomUserSetup.firedOffsets.push({ xOffset, yOffset, hit })
            this.lap++
            this.roomLapTimer = 0
        },
      )
      this.hub.connection.on('PlacingTimerTick', (timer: number) => {
        this.roomPlacingTimer = timer
      })
      this.hub.connection.on('PlacingTimerTimeout', () => {
        if (this.room === null) return
        this.room.state = ERoomState.archived
        this.hasBeenCanceled = true
      })
      this.hub.connection.on('LapTimerTick', (timer: number) => {
        this.roomLapTimer = timer
      })
      this.hub.connection.on('LapTimerTimeout', (newLapCount: number) => {
        if (this.isCurrentUserTurn) {
          Notify.create({
            type: 'negative',
            message: 'Temps écoulé pour ce tour',
          })
        }
        this.lap = newLapCount
      })
      this.hub.connection.on('PlayerWon', (playerId: number) => {
        this.playerWon = true
        setTimeout(() => {
          if (this.room === null) return
          this.winnerId = playerId
          this.room.state = ERoomState.archived
        }, 1500)
      })
      await this.hub.connect()
      await this.hub.invokeCommand('JoinRoomOpponentChannel', roomGuid)
    },
    async unregister() {
      await this.hub.disconnect()
      if (this.room === null) return
      await leaveRoomAPI(this.room.guid)
      this.$reset()
    },
    async setupRoom(roomGuid: string) {
      this.room = await getRoomAsOpponentByGuidAPI(roomGuid)
      if (this.room.state === ERoomState.placing || this.room.state === ERoomState.playing) {
        this.roomUserSetup.ships =
          this.room.userSetup?.ships.map((ship) => {
            return convertShipToShipPlacement(ship)
          }) ?? []
      }
      if (this.room.state === ERoomState.playing) {
        this.roomUserSetup.firedOffsets = this.room.userFiredOffsets ?? []
        this.roomOpponentSetup.firedOffsets = this.room.opponentFiredOffsets ?? []
      }
    },
    place(boatPlacements: IShipPlacement[]) {
      this.roomUserSetup.ships = boatPlacements
    },
    async fire(xOffset: number, yOffset: number) {
      if (this.room === null) return
      const fireResponse = await fireInRoomAPI(this.room.guid, xOffset, yOffset)
      if (fireResponse === EHitType.hitShipAndDrawned) {
        const starStore = useShootingStarStore()
        starStore.triggerStarfall()
      }
      this.roomOpponentSetup.firedOffsets.push({ xOffset, yOffset, hit: fireResponse })
      this.lap++
      this.roomLapTimer = 0
    },
    getPlayerPseudoById(playerId: number): string {
      if (this.room === null) return ''
      if (this.room.playerOne.id === playerId) return this.room.playerOne.pseudo
      if (this.room.playerTwo?.id === playerId) return this.room.playerTwo?.pseudo ?? ''
      return ''
    },
    getOpponentHit(xOffset: number, yOffset: number): IOffsetsWithHit | null {
      if (this.room === null || this.roomOpponentSetup === null) return null
      return (
        this.roomOpponentSetup.firedOffsets.find((firedOffset) => {
          return firedOffset.xOffset === xOffset && firedOffset.yOffset === yOffset
        }) ?? null
      )
    },
    getSelfHit(xOffset: number, yOffset: number): IOffsetsWithHit | null {
      if (this.room === null || this.roomUserSetup === null) return null
      return (
        this.roomUserSetup.firedOffsets.find((firedOffset) => {
          return firedOffset.xOffset === xOffset && firedOffset.yOffset === yOffset
        }) ?? null
      )
    },
    getSelfBoat(xOffset: number, yOffset: number): IShipPlacement | null {
      if (this.roomUserSetup === null) return null
      return (
        this.roomUserSetup.ships.find((userShip) => {
          const userShipFirstOffset = userShip.offsets.at(0)!
          return userShipFirstOffset.xOffset === xOffset && userShipFirstOffset.yOffset === yOffset
        }) ?? null
      )
    },
  },
})
