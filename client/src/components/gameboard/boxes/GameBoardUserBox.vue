<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store'
import { computed } from 'vue'
import Ship from '../ShipComponent.vue'

const propsComponents = defineProps<{
  xOffset: number
  yOffset: number
}>()

const roomStore = useRoomOpponentStore()

const hitClasses = computed(() => {
  let classes = ''
  const selfHit = roomStore.getSelfHit(propsComponents.xOffset, propsComponents.yOffset)
  if (selfHit !== null && selfHit.hit === true) classes += ' gameboard-box-hit-ship'
  if (selfHit !== null && selfHit.hit === false) classes += ' gameboard-box-hit'
  return classes
})
const hasBoat = computed(() => {
  return roomStore.getSelfBoat(propsComponents.xOffset, propsComponents.yOffset)
})
</script>

<template>
  <div class="gameboard-box gameboard-box-player">
    <Ship v-if="hasBoat" :ship-placement="hasBoat" />
    <div :class="hitClasses"></div>
  </div>
</template>

<style>
.gameboard-box-player {
  background-color: #587e74b0;
}
</style>
