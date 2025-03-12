import { defineStore } from 'pinia'
import { uid } from 'quasar'
import { placeInRoomAPI } from 'src/api/rooms.api'

export const GRID_SIZE = 8 + 1
// TODO A l'avenir, générer la list de ship selon un mode et une taille de grid
const defaultVerticalBoatList: IShipPlacement[] = [
  {
    guid: uid(),
    type: 'big',
    orientation: 'vertical',
    enabled: true,
    offsets: [
      { xOffset: 0, yOffset: 0 },
      { xOffset: 0, yOffset: 1 },
      { xOffset: 0, yOffset: 2 },
      { xOffset: 0, yOffset: 3 },
      { xOffset: 0, yOffset: 4 },
    ],
  },
  {
    guid: uid(),
    type: 'medium',
    orientation: 'vertical',
    enabled: true,
    offsets: [
      { xOffset: 0, yOffset: 0 },
      { xOffset: 0, yOffset: 1 },
      { xOffset: 0, yOffset: 2 },
      { xOffset: 0, yOffset: 3 },
    ],
  },
  {
    guid: uid(),
    type: 'small',
    orientation: 'vertical',
    enabled: true,
    offsets: [
      { xOffset: 0, yOffset: 0 },
      { xOffset: 0, yOffset: 1 },
      { xOffset: 0, yOffset: 2 },
    ],
  },
]
const defaultHorizontalBoatList: IShipPlacement[] = [
  {
    guid: uid(),
    type: 'big',
    orientation: 'horizontal',
    enabled: true,
    offsets: [
      { xOffset: 0, yOffset: 0 },
      { xOffset: 1, yOffset: 0 },
      { xOffset: 2, yOffset: 0 },
      { xOffset: 3, yOffset: 0 },
      { xOffset: 4, yOffset: 0 },
    ],
  },
  {
    guid: uid(),
    type: 'medium',
    orientation: 'horizontal',
    enabled: true,
    offsets: [
      { xOffset: 0, yOffset: 0 },
      { xOffset: 1, yOffset: 0 },
      { xOffset: 2, yOffset: 0 },
      { xOffset: 3, yOffset: 0 },
    ],
  },
  {
    guid: uid(),
    type: 'small',
    orientation: 'horizontal',
    enabled: true,
    offsets: [
      { xOffset: 0, yOffset: 0 },
      { xOffset: 1, yOffset: 0 },
      { xOffset: 2, yOffset: 0 },
    ],
  },
]

export interface IShipPlacement {
  guid: string
  type: 'small' | 'medium' | 'big'
  enabled: boolean
  orientation: 'horizontal' | 'vertical'
  offsets: {
    xOffset: number
    yOffset: number
  }[]
}

export const useRoomPlacementStore = defineStore('room-placement', {
  state: () => ({
    roomGuid: <string | null>null,
    hasSubmitPlacement: <boolean>false,
    verticalBoatList: <IShipPlacement[]>[],
    horizontalBoatList: <IShipPlacement[]>[],
    currentPlacements: <IShipPlacement[]>[],
  }),
  getters: {
    isPlacementValid(): boolean {
      return this.currentPlacements.length === 3
    },
  },
  actions: {
    setup(roomGuid: string) {
      this.$reset()
      this.roomGuid = roomGuid
      this.verticalBoatList = JSON.parse(JSON.stringify(defaultVerticalBoatList))
      this.horizontalBoatList = JSON.parse(JSON.stringify(defaultHorizontalBoatList))
    },
    tryAddToCurrentPlacement(boat: IShipPlacement, xOffset: number, yOffset: number): boolean {
      let invalid = false
      const currentPlacementCount = this.currentPlacements.length
      this.currentPlacements = this.currentPlacements.filter((currentPlacement) => {
        return currentPlacement.guid !== boat.guid
      })
      const calculatedOffsets = boat.offsets.map((offset) => {
        return {
          xOffset: offset.xOffset + xOffset,
          yOffset: offset.yOffset + yOffset,
        }
      })
      // next time use a new Set
      for (let i = 0; i < calculatedOffsets.length; i++) {
        const calculatedOffset = calculatedOffsets[i]!
        if (
          calculatedOffset.xOffset > 7 ||
          calculatedOffset.xOffset < 0 ||
          calculatedOffset.yOffset > 7 ||
          calculatedOffset.yOffset < 0
        ) {
          invalid = true
          break
        }
        for (let j = 0; j < this.currentPlacements.length; j++) {
          const alreadyPlacedShip = this.currentPlacements[j]!
          for (let k = 0; k < alreadyPlacedShip.offsets.length; k++) {
            const alreadyPlacedOffset = alreadyPlacedShip.offsets[k]!
            if (
              alreadyPlacedOffset.xOffset === calculatedOffset.xOffset &&
              alreadyPlacedOffset.yOffset === calculatedOffset.yOffset
            ) {
              invalid = true
              break
            }
          }
          if (invalid) break
        }
      }
      if (invalid) {
        if (currentPlacementCount !== this.currentPlacements.length) {
          this.setEnableBoatType(boat.type, true)
        }
        return false
      }
      this.setEnableBoatType(boat.type, false)
      this.currentPlacements.push({
        ...boat,
        offsets: calculatedOffsets,
      })
      return true
    },
    setEnableBoatType(boatType: IShipPlacement['type'], state: boolean) {
      this.verticalBoatList.forEach((verticalBoat) => {
        if (verticalBoat.type === boatType) {
          verticalBoat.enabled = state
        }
      })
      this.horizontalBoatList.forEach((horizontalBoat) => {
        if (horizontalBoat.type === boatType) {
          horizontalBoat.enabled = state
        }
      })
    },
    async submitPlacement() {
      if (this.roomGuid === null) return
      await placeInRoomAPI(this.roomGuid, {
        shipsOffsets: this.currentPlacements,
      })
      this.hasSubmitPlacement = true
    },
    clear() {
      this.$reset()
    },
  },
})
