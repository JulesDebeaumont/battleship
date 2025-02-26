import { api } from 'src/boot/axios'

const roomsUrl = 'rooms'

export async function getAllRoomsAPI(): Promise<IGetAllRoomsFrontDTO[]> {
  return (await api.get(`${roomsUrl}`)).data
}
export async function createRoomAPI(): Promise<number> {
  return (await api.post(`${roomsUrl}`)).data
}
export async function getRoomByIdAPI(roomId: number): Promise<IGetRoomByIdFrontDTO> {
  return (await api.get(`${roomsUrl}/${roomId}`)).data
}

interface IRoom {
  id: number
  state: TRoomState
  createdAt: Date
  startedAt: Date
  endedAt: Date
}

export interface IGetAllRoomsFrontDTO extends IRoom {
  playerOne: {
    id: number
    fullName: string
  }
  playerTwo: {
    id: number
    fullName: string
  }
}
export interface IGetRoomByIdFrontDTO extends IRoom {
  playerOne: {
    id: number
    fullName: string
  }
  playerTwo: {
    id: number
    fullName: string
  }
  moves: {
    id: number;
    playerId: number;
    xOffset: number;
    yOffset: number;
    lap: number;
  }[]
}
export type TRoomState = 'playing' | 'pending' | 'archived'
