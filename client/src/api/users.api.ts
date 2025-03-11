import { api } from 'src/boot/axios'

const userUrl = 'users'

export async function updatePseudoAPI(pseudo: string): Promise<void> {
  return (await api.put(`${userUrl}/pseudo`, { pseudo })).data
}
export async function getProfileAPI(): Promise<IUserProfilDto> {
  return (await api.get(`${userUrl}/me`)).data
}
export async function getLeaderboardAPI(): Promise<IUserLeaderboardDto[]> {
  return (await api.get(`${userUrl}/leaderboard`)).data
}

export interface IUserProfilDto {
  idRes: string
  pseudo: string
  gameCount: number
  winCount: number
  looseCount: number
  shipDestroyed: number
  lapPlayed: number
  rankLeaderboard: number
  level: number
  experienceScopeNextLevel: number
  experienceRequiredNextLevel: number
}
export interface IUserLeaderboardDto {
  id: number
  pseudo: string
  winCount: number
  level: number
}
