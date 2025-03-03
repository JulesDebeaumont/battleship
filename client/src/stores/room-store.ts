import { defineStore } from 'pinia'

export const useRoomStore = defineStore('room', {
  state: () => ({
    user: <IUser | null>null,

  }),
  getters: {

  },
  actions: {
    async trySetupTokenFromCookies() {
      if (Cookies.has(COOKIE_TOKEN)) {
        await this.authenticate(Cookies.get(COOKIE_TOKEN))
      }
    },
  },
})
