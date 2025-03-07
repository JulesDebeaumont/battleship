<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import { computed } from 'vue';

const propsComponents = defineProps<{
    xOffset: number
    yOffset: number
}>()

const roomStore = useRoomOpponentStore()

async function clickBox() {
    if (!roomStore.isCurrentUserTurn || roomStore.getOpponentHit(propsComponents.xOffset, propsComponents.yOffset) !== null) {
        return
    }
    await roomStore.fire(propsComponents.xOffset, propsComponents.yOffset)
}

const boxClasses = computed(() => {
    let classes = ''
    if (roomStore.isCurrentUserTurn && roomStore.getOpponentHit(propsComponents.xOffset, propsComponents.yOffset) === null) classes += ' gameboard-box-interactive'
    return classes
})
const hitClasses = computed(() => {
    let classes = ''
    const opponentHit = roomStore.getOpponentHit(propsComponents.xOffset, propsComponents.yOffset)
    if (opponentHit !== null && opponentHit.hit === true) classes += ' gameboard-box-hit-ship'
    if (opponentHit !== null && opponentHit.hit === false) classes += ' gameboard-box-hit'
    return classes
})
</script>

<template>
    <div class="gameboard-box" @click="clickBox" :class="boxClasses">
        <div :class="hitClasses">
        </div>
    </div>
</template>
