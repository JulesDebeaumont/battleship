<script setup lang="ts">
import { EHitType } from 'src/api/rooms.api';
import { useRoomSpectatorStore } from 'src/stores/spectator-room-store';
import { computed } from 'vue';
import ShipComponent from '../ShipComponent.vue';

const propsComponents = defineProps<{
    xOffset: number
    yOffset: number
    playerOne: boolean
}>()

const spectatorStore = useRoomSpectatorStore()

const hitClasses = computed(() => {
    let classes = ''
    const hit = spectatorStore.getHit(propsComponents.playerOne, propsComponents.xOffset, propsComponents.yOffset)
    if (hit !== null && hit === EHitType.hitShipAndDrawned) classes += ' gameboard-box-hit-ship-drawned'
    if (hit !== null && hit === EHitType.hitShip) classes += ' gameboard-box-hit-ship'
    if (hit !== null && hit === EHitType.hitNothing) classes += ' gameboard-box-hit'
    return classes
})
const hasBoat = computed(() => {
  return spectatorStore.getBoat(propsComponents.playerOne, propsComponents.xOffset, propsComponents.yOffset)
})
</script>

<template>
    <div class="gameboard-box gameboard-box-spectator">
        <div :class="hitClasses">
            <ShipComponent v-if="hasBoat" :ship-placement="hasBoat" />
        </div>
    </div>
</template>

<style>
.gameboard-box-spectator {
    background-color: #587e74b0;
    transition: all 0.1s;
}

.gameboard-box-spectator:hover {
    background-color: rgba(165, 255, 255, 0.425);
}
</style>
