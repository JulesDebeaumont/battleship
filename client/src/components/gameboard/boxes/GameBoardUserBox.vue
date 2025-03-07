<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import { computed } from 'vue';

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
const shipClasses = computed(() => {
    let classes = ''
    const selfBoat = roomStore.getSelfBoat(propsComponents.xOffset, propsComponents.yOffset)
    if (selfBoat !== null) classes += ` ship ${selfBoat.classes}`
    return classes
})
</script>

<template>
    <div class="gameboard-box">
        <div :class="shipClasses"></div>
        <div :class="hitClasses"></div>
    </div>
</template>
