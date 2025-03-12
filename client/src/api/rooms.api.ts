import { api } from 'src/boot/axios'
import type { IShipPlacement } from 'src/stores/room-placement-store'

const roomsUrl = 'rooms'

export async function getAvailableRoomsAPI(): Promise<IRoomFromListDto[]> {
  return (await api.get(`${roomsUrl}`)).data
}
export async function createRoomAPI(): Promise<string> {
  return (await api.post(`${roomsUrl}/new`)).data.guid
}
export async function getRoomAsOpponentByGuidAPI(roomGuid: string): Promise<IRoomOpponentDto> {
  return (await api.get(`${roomsUrl}/${roomGuid}/as-opponent`)).data
}
export async function getRoomAsSpectatorByGuidAPI(roomGuid: string): Promise<IRoomSpectatorDto> {
  return (await api.get(`${roomsUrl}/${roomGuid}/as-spectator`)).data
}
export async function placeInRoomAPI(
  roomGuid: string,
  shipsPlacements: IShipPlacementDto,
): Promise<void> {
  return (await api.post(`${roomsUrl}/${roomGuid}/place`, shipsPlacements)).data
}
export async function fireInRoomAPI(
  roomGuid: string,
  xOffset: number,
  yOffset: number,
): Promise<boolean> {
  return (
    await api.post(`${roomsUrl}/${roomGuid}/fire`, {
      xOffset,
      yOffset,
    })
  ).data.hit
}
export async function leaveRoomAPI(roomGuid: string): Promise<void> {
  return (await api.post(`${roomsUrl}/${roomGuid}/leave`)).data
}
export async function joinRoomAsOpponentAPI(roomGuid: string): Promise<void> {
  return (await api.post(`${roomsUrl}/${roomGuid}/join-as-opponent`)).data
}
export async function JoinRoomAsSpectatorAPI(roomGuid: string): Promise<void> {
  return (await api.post(`${roomsUrl}/${roomGuid}/join-as-spectator`)).data
}

export interface IRoomFromListDto {
  guid: string
  state: ERoomState
  playerOne: IRoomPlayer
  playerTwo: IRoomPlayer | null
  createdAt: Date
  startedAt: Date | undefined
}
export interface IRoomOpponentDto {
  guid: string
  state: ERoomState
  playerOne: IRoomPlayer
  userSetup: IRoomSetup | null
  userFiredOffsets: IOffsetsWithHit[] | null
  playerTwo: IRoomPlayer | null
  opponentFiredOffsets: IOffsetsWithHit[] | null
  createdAt: Date
  startedAt: Date | undefined
}
export interface IRoomSpectatorDto {
  guid: string
  state: ERoomState
  playerOne: IRoomPlayer
  playerOneSetup: IRoomSetup | null
  playerTwoSetup: IRoomSetup | null
  playerTwo: IRoomPlayer | null
  createdAt: Date
  startedAt: Date | undefined
}
export interface IOffsets {
  xOffset: number
  yOffset: number
}
export interface IOffsetsWithHit {
  xOffset: number
  yOffset: number
  hit: boolean
}
interface IRoomPlayer {
  id: number
  pseudo: string
}
interface IRoomSetup {
  ships: IShip[]
  firedOffsets: IOffsetsWithHit[]
}
interface IShip {
  positions: IOffsetsWithHit[]
  guid: string
  type: IShipPlacement['type']
  orientation: IShipPlacement['orientation']
}
export interface IShipPlacementDto {
  shipsOffsets: Omit<IShipPlacement, 'enabled'>[]
}
export enum ERoomState {
  playing = 0,
  pending = 1,
  placing = 2,
  archived = 3,
}
