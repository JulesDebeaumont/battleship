import { defineStore } from 'pinia'
import { Notify } from 'quasar'
import type { IOffsetsWithHit, IRoomOpponentDto } from 'src/api/rooms.api'
import {
  ERoomState,
  fireInRoomAPI,
  getRoomAsOpponentByGuidAPI,
  leaveRoomAPI,
} from 'src/api/rooms.api'
import { useSignalR } from 'src/hooks/use-signalr'
import { useUserStore } from './user-store'
import type { IShipPlacement } from './room-placement-store'

export const GRID_SIZE = 8 + 1

interface IRoomUserSetup {
  ships: IShipPlacementStrict[]
  firedOffsets: IOffsetsWithHit[]
}
interface IShipPlacementStrict {
  guid: string
  classes: string
  offsets: {
    xOffset: number
    yOffset: number
  }[]
}
interface IRoomOpponentSetup {
  firedOffsets: IOffsetsWithHit[]
}

export const useRoomSpectatorStore = defineStore('room-spectator', {
  state: () => ({
    hub: useSignalR('rooms-on'),
    userStore: useUserStore(),
    room: <IRoomOpponentDto | null>null,
    roomUserSetup: <IRoomUserSetup>{
      ships: [],
      firedOffsets: [],
    },
    roomOpponentSetup: <IRoomOpponentSetup>{
      firedOffsets: [],
    },
    playerOneReady: <boolean>false,
    playerTwoReady: <boolean>false,
    roomTimer: <number>0,
    lap: <number>1,
    winnerId: <number | null>null,
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
      this.hub.connection.on('OpponentJoined', (room: IRoomOpponentDto, playerId: number) => {
        this.room = room
        if (playerId !== this.userStore.user!.id) {
          Notify.create({
            message: 'Un adversaire à rejoint la partie',
          })
        }
        if (this.room.state === ERoomState.placing) {
          Notify.create({
            message: 'Placer vos bateaux !',
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
          message: `${opponentPseudo} est prêt !`,
        })
      })
      this.hub.connection.on('GameOn', () => {
        if (this.room === null) return
        this.room.state = ERoomState.playing
        Notify.create({
          message: 'Game on bitch',
        })
      })
      this.hub.connection.on(
        'Move',
        (playerId: number, xOffset: number, yOffset: number, hit: boolean) => {
          if (this.room === null || this.userStore.user === null || this.room.playerTwo === null)
            return
          if (playerId === this.userStore.user.id) return
          this.roomUserSetup.firedOffsets.push({ xOffset, yOffset, hit })
          this.lap++
          this.roomTimer = 0
        },
      )
      this.hub.connection.on('TimerTick', (timer: number) => {
        this.roomTimer = timer
      })
      this.hub.connection.on('TimerTimeout', (newLapCount: number) => {
        Notify.create({
          message: 'Temps écoulé pour ce tour',
        })
        this.lap = newLapCount
      })
      this.hub.connection.on('PlayerWon', (playerId: number) => {
        this.winnerId = playerId
        if (this.room === null) return
        this.room.state = ERoomState.archived
      })
      await this.hub.connect()
      await this.hub.invokeCommand('JoinRoomOpponentChannel', roomGuid)
    },
    async unregister() {
      console.log('unregister 1')
      await this.hub.disconnect()
      if (this.room === null) return
      await leaveRoomAPI(this.room.guid)
      this.$reset()
    },
    async setupRoom(roomGuid: string) {
      this.room = await getRoomAsOpponentByGuidAPI(roomGuid)
      if (this.room.state === ERoomState.playing) { // in case of refresh page during play
        this.roomUserSetup = {
          ships: this.room.userSetup?.ships.map((ship) => {
            return {
              guid: ship.guid,
              classes: ship.classes,
              offsets: ship.positions.map((shipPosition) => {
                return {
                  xOffset: shipPosition.xOffset,
                  yOffset: shipPosition.yOffset
                }
              })
            }
          }) ?? [],
          firedOffsets: this.room.userFiredOffsets ?? []
        }
        this.roomOpponentSetup = {
          firedOffsets: this.room.opponentFiredOffsets ?? []
        }
      }
    },
    place(boatPlacements: IShipPlacement[]) {
      this.roomUserSetup.ships = boatPlacements
    },
    async fire(xOffset: number, yOffset: number) {
      if (this.room === null) return
      const fireResponse = await fireInRoomAPI(this.room.guid, xOffset, yOffset)
      this.roomOpponentSetup.firedOffsets.push({ xOffset, yOffset, hit: fireResponse })
      this.lap++
      this.roomTimer = 0
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
    getSelfBoat(xOffset: number, yOffset: number): IShipPlacementStrict | null {
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
