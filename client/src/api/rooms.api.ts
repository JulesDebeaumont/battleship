import { api } from 'src/boot/axios'

const roomsUrl = 'rooms'

export async function getAvailableRoomsAPI(): Promise<IRestrictedRoomDto[]> {
  return (await api.get(`${roomsUrl}`)).data
}
export async function createRoomAPI(): Promise<string> {
  return (await api.post(`${roomsUrl}/new`)).data
}
export async function getRoomByGuidAPI(roomGuid: string): Promise<IRestrictedRoomDto> {
  return (await api.get(`${roomsUrl}/${roomGuid}`)).data
} 
export async function placeInRoomAPI(roomGuid: string, shipOffsets: number[][][]): Promise<void> {
  return (
    await api.post(`${roomsUrl}/${roomGuid}/place`, {
      shipOffsets,
    })
  ).data
}
export async function fireInRoomAPI(roomGuid: string, xOffset: number, yOffset: number): Promise<void> {
  return (
    await api.post(`${roomsUrl}/${roomGuid}/fire`, {
      xOffset,
      yOffset
    })
  ).data
}
export async function leaveRoomAPI(roomGuid: string): Promise<void> {
  return (
    await api.post(`${roomsUrl}/${roomGuid}/leave`)
  ).data
}
export async function JoinRoomAsOpponentAPI(roomGuid: string): Promise<void> {
  return (
    await api.post(`${roomsUrl}/${roomGuid}/join-as-opponent`,)
  ).data
}
export async function JoinRoomAsSpectatorAPI(roomGuid: string): Promise<void> {
  return (
    await api.post(`${roomsUrl}/${roomGuid}/join-as-spectator`)
  ).data
}

export interface IRestrictedRoomDto {
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
