<script setup lang="ts">
import { EHitType } from 'src/api/rooms.api';
import { useRoomFightStore } from 'src/stores/room-fight-store';
import { computed } from 'vue';

const propsComponents = defineProps<{
    xOffset: number
    yOffset: number
}>()

const roomStore = useRoomFightStore()

async function clickBox() {
    if (!roomStore.isCurrentUserTurn || roomStore.getOpponentHit(propsComponents.xOffset, propsComponents.yOffset) !== null) {
        return
    }
    await roomStore.fire(propsComponents.xOffset, propsComponents.yOffset)
}

const boxClasses = computed(() => {
    let classes = ''
    if (roomStore.isCurrentUserTurn && roomStore.getOpponentHit(propsComponents.xOffset, propsComponents.yOffset) === null) classes += ' gameboard-box-opponent-interactive'
    return classes
})
const hitClasses = computed(() => {
    let classes = ''
    const opponentHit = roomStore.getOpponentHit(propsComponents.xOffset, propsComponents.yOffset)
    if (opponentHit !== null && opponentHit.hit === EHitType.hitShipAndDrawned) classes += ' gameboard-box-hit-ship-drawned'
    if (opponentHit !== null && opponentHit.hit === EHitType.hitShip) classes += ' gameboard-box-hit-ship'
    if (opponentHit !== null && opponentHit.hit === EHitType.hitNothing) classes += ' gameboard-box-hit'
    return classes
})
</script>

<template>
    <div class="gameboard-box gameboard-box-opponent" @click="clickBox" :class="boxClasses">
        <div :class="hitClasses">
        </div>
    </div>
</template>

<style>
.gameboard-box-opponent {
    background-color: #587e74b0;
    transition: all 0.1s;
}
.gameboard-box-opponent:hover {
    background-color: rgba(165, 255, 255, 0.425);
    background-size: contain;
}
.gameboard-box-opponent-interactive {
    background-color: #ceffffb7;
}
.gameboard-box-opponent-interactive:hover {
    background-color: #85ffffe0;
    cursor: url('/images/scope.png') 32 32, auto;
    scale:  110%;
}
</style>
