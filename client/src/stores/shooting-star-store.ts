import { defineStore } from 'pinia'

export const useShootingStarStore = defineStore('shooting-star', {
  state: () => ({
    starfallCallback: <(() => void) | null> null
  }),
  actions: {
    triggerStarfall() {
      if (!this.starfallCallback) return
      this.starfallCallback()
    },
    register(starfallCallback: () => void) {
      this.starfallCallback = starfallCallback;
    },
    unregister() {
      this.$reset()
    }
  }
})
