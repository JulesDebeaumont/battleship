import { api } from 'src/boot/axios'

const roomsUrl = 'rooms'

export async function getAvailableRoomsAPI(): Promise<IRestrictedRoomDto[]> {
  return (await api.get(`${roomsUrl}`)).data
}

interface IRestrictedRoomDto {
  guid: string
  state: TRoomState
  playerOne: {
    id: number
    pseudo: string
    idres: string
  }
  playerTwo: {
    id: number
    pseudo: string
    idres: string
  }
  createdAt: Date
  startedAt: Date
}
export type TRoomState = 'playing' | 'pending' | 'archived' | 'placing'
