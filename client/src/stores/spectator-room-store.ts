import { defineStore } from 'pinia'
import { Notify } from 'quasar'
import type { IRoomSpectatorDto, EHitType } from 'src/api/rooms.api'
import { ERoomState, getRoomAsSpectatorByGuidAPI } from 'src/api/rooms.api'
import { useSignalR } from 'src/hooks/use-signalr'
import type { IShipPlacement } from './room-placement-store'
import { convertShipToShipPlacement } from 'src/utils/misc'

export const useRoomSpectatorStore = defineStore('room-spectator', {
  state: () => ({
    hub: useSignalR('rooms-on'),
    room: <IRoomSpectatorDto | null>null,
    playerOneReady: <boolean>false,
    playerTwoReady: <boolean>false,
    roomPlacingTimer: <number>0,
    roomLapTimer: <number>0,
    hasBeenCanceled: <boolean>false,
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
    getWinnerString(): string | null {
      if (this.room === null) return null
      if (this.hasBeenCanceled) return 'La partie est annulée'
      let pseudo: string | null = null
      if (this.winnerId === this.room.playerOne.id) pseudo = this.room.playerOne.pseudo
      if (this.winnerId === this.room.playerTwo?.id) pseudo = this.room.playerTwo.pseudo
      return `${pseudo} à gagner !`
    },
  },
  actions: {
    async register(roomGuid: string) {
      this.$reset()
      await this.setupRoom(roomGuid)
      this.hub.connection.on('PlayerReady', (playerId: number) => {
        if (this.room === null || this.room.playerTwo === null) return
        if (this.room.playerOne.id === playerId) {
          this.playerOneReady = true
        }
        if (this.room.playerTwo.id === playerId) {
          this.playerTwoReady = true
        }
      })
      this.hub.connection.on('GameOn', (roomData: IRoomSpectatorDto) => {
        if (this.room === null) return
        this.room = roomData
        Notify.create({
          color: 'positive',
          message: 'La bataille commence !',
        })
      })
      this.hub.connection.on(
        'Move',
        (playerId: number, xOffset: number, yOffset: number, hit: EHitType) => {
          if (this.room === null || this.room.playerOne === null || this.room.playerTwo === null)
            return
          const roomSetup =
            this.room.playerOne.id === playerId
              ? this.room.playerTwoSetup
              : this.room.playerOneSetup
          if (!roomSetup) return
          roomSetup.firedOffsets.push({ xOffset, yOffset, hit })
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
        Notify.create({
          type: 'negative',
          message: 'Temps écoulé pour ce tour',
        })
        this.lap = newLapCount
      })
      this.hub.connection.on('PlayerWon', (playerId: number) => {
        setTimeout(() => {
          if (this.room === null) return
          this.winnerId = playerId
          this.room.state = ERoomState.archived
        }, 1500)
      })
      await this.hub.connect()
      await this.hub.invokeCommand('JoinRoomSpectatorChannel', roomGuid)
    },
    async unregister() {
      await this.hub.disconnect()
      if (this.room === null) return
      this.$reset()
    },
    async setupRoom(roomGuid: string) {
      this.room = await getRoomAsSpectatorByGuidAPI(roomGuid)
    },
    getHit(playerOne: boolean, xOffset: number, yOffset: number): EHitType | null {
      if (
        this.room === null ||
        this.room.playerOneSetup === null ||
        this.room.playerTwoSetup === null
      )
        return null
      const roomSetup = playerOne ? this.room.playerOneSetup : this.room.playerTwoSetup
      return (
        roomSetup.firedOffsets.find((userShip) => {
          return userShip.xOffset === xOffset && userShip.yOffset === yOffset
        })?.hit ?? null
      )
    },
    getBoat(playerOne: boolean, xOffset: number, yOffset: number): IShipPlacement | null {
      if (
        this.room === null ||
        this.room.playerOneSetup === null ||
        this.room.playerTwoSetup === null
      )
        return null
      const roomSetup = playerOne ? this.room.playerOneSetup : this.room.playerTwoSetup
      const ship =
        roomSetup.ships.find((userShip) => {
          const userShipFirstOffset = userShip.positions.at(0)!
          return userShipFirstOffset.xOffset === xOffset && userShipFirstOffset.yOffset === yOffset
        }) ?? null
      if (!ship) return null
      return convertShipToShipPlacement(ship)
    },
  },
})
