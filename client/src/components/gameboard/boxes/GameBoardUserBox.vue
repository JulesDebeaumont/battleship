<script setup lang="ts">
import { useRoomFightStore } from 'src/stores/room-fight-store'
import { computed } from 'vue'
import ShipComponent from '../ShipComponent.vue'
import { EHitType } from 'src/api/rooms.api';

const propsComponents = defineProps<{
  xOffset: number
  yOffset: number
}>()

const roomStore = useRoomFightStore()

const hitClasses = computed(() => {
  let classes = ''
  const selfHit = roomStore.getSelfHit(propsComponents.xOffset, propsComponents.yOffset)
  if (selfHit !== null && selfHit.hit === EHitType.hitShipAndDrawned) classes += ' gameboard-box-hit-ship-drawned'
  if (selfHit !== null && selfHit.hit === EHitType.hitShip) classes += ' gameboard-box-hit-ship'
  if (selfHit !== null && selfHit.hit === EHitType.hitNothing) classes += ' gameboard-box-hit'
  return classes
})
const hasBoat = computed(() => {
  return roomStore.getSelfBoat(propsComponents.xOffset, propsComponents.yOffset)
})
</script>

<template>
  <div class="gameboard-box gameboard-box-player">
    <ShipComponent v-if="hasBoat" :ship-placement="hasBoat" />
    <div :class="hitClasses"></div>
  </div>
</template>

<style>
.gameboard-box-player {
  background-color: #587e74b0;
}
</style>
