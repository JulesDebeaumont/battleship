import { api } from 'src/boot/axios'

const userUrl = 'users'

export async function updatePseudoAPI(pseudo: string): Promise<void> {
  return (await api.put(`${userUrl}/pseudo`, { pseudo })).data
}
export async function getProfileAPI(): Promise<IUserProfilDto> {
  return (await api.get(`${userUrl}/me`)).data
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
  experience: number
}
