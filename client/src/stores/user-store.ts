import { jwtDecode } from 'jwt-decode'
import { defineStore } from 'pinia'
import { Cookies } from 'quasar'
import { loginAPI } from 'src/api/login.api'

interface IUser {
  id: number
  pseudo: string
}
interface IParsedToken {
  exp: number
  user: IUser
}
const COOKIE_TOKEN = 'SpaceShipBattle-AccessToken'
const COOKIE_PATH = '/'

export const useUserStore = defineStore('user', {
  state: () => ({
    user: <IUser | null>null,
    token: <string | null>null,
    tokenExpiration: <number | null>null,
  }),
  getters: {
    isConnected(): boolean {
      return this.user !== null
    },
    istokenExpired(): boolean {
      if (this.tokenExpiration === null) return true
      if (new Date() > new Date(this.tokenExpiration * 1000)) return true
      return false
    },
  },
  actions: {
    trySetupTokenFromCookies() {
      if (Cookies.has(COOKIE_TOKEN)) {
        this.authenticate(Cookies.get(COOKIE_TOKEN))
      }
    },
    clear() {
      this.user = null
      this.token = null
      Cookies.remove(COOKIE_TOKEN, { path: COOKIE_PATH })
    },
    askCasTicket() {
      window.location.href = process.env.CAS_URL + window.location.origin
    },
    async askServerToken(casTicket: string) {
      try {
        const token = (await loginAPI(casTicket)).token;
        this.authenticate(token);
      } catch (error) {
        console.log(error)
      }
    },
    authenticate(token: string) {
      this.token = token
      const parsedToken = jwtDecode<IParsedToken>(token)
      this.user = parsedToken.user
      this.tokenExpiration = parsedToken.exp
      Cookies.set(COOKIE_TOKEN, token, { sameSite: 'Lax', path: COOKIE_PATH })
    },
  },
})
