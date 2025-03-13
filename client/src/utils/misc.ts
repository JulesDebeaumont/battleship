import type { IRoomSetup } from 'src/api/rooms.api'
import type { IShipPlacement } from 'src/stores/room-placement-store'

export function convertShipToShipPlacement(ship: IRoomSetup['ships'][number]): IShipPlacement {
  return {
    guid: ship.guid,
    type: ship.type,
    enabled: true,
    orientation: ship.orientation,
    offsets: ship.positions.map((position) => {
      return {
        xOffset: position.xOffset,
        yOffset: position.yOffset,
      }
    }),
  }
}
