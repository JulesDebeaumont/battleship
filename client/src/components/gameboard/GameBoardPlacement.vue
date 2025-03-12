<script setup lang="ts">
import { useRoomFightStore } from 'src/stores/room-fight-store';
import GameBoardGeneric from './GameBoardGeneric.vue';
import GameBoardPlacementBox from './boxes/GameBoardPlacementBox.vue';
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoomPlacementStore } from 'src/stores/room-placement-store';
import GameBoardShipToolBox from './GameBoardShipToolBox.vue';
import { useRoute } from 'vue-router';
import SpaceButton from '../general/SpaceButton.vue';
import WaitingButton from '../general/WaitingButton.vue';
import { TIMEOUT_PLACEMENT } from 'src/utils/board-utils';

const roomStore = useRoomFightStore()
const placementStore = useRoomPlacementStore()
const route = useRoute()
const roomGuid = route.params.guid!.toString()

async function validatePlacement() {
    roomStore.place(placementStore.currentPlacements)
    await placementStore.submitPlacement()
}

const disableSubmitPlacement = ref(true)

watch(() => placementStore.isPlacementValid, (newValue) => {
    disableSubmitPlacement.value = !newValue
})
watch(() => placementStore.hasSubmitPlacement, (newValue) => {
    disableSubmitPlacement.value = newValue
})

const hasPlaced = computed(() => {
    return roomStore.roomUserSetup.ships.length > 0
})
const labelWaitingForOpponent = computed(() => {
    return "En attente de " + roomStore.getOpponentPseudo + ".."
})
const labelPlacingTimerButton = computed(() => {
    const invertedTimer = TIMEOUT_PLACEMENT - 1 - roomStore.roomPlacingTimer
    return `Temps restant : ${invertedTimer > 0 ? invertedTimer : 0}`
})

onMounted(() => {
    placementStore.setup(roomGuid)
})
onUnmounted(() => {
    placementStore.clear()
})
</script>

<template>
    <div class="flex flex-center column">

        <WaitingButton :label="`Placements des vaisseaux : ${labelPlacingTimerButton}`" />

        <GameBoardGeneric class="q-mb-md">
            <template v-slot:boxes="boxSlot">
                <GameBoardPlacementBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset"
                    :key="`${boxSlot.xOffset}-${boxSlot.yOffset}`" />
            </template>
        </GameBoardGeneric>

        <SpaceButton v-if="!hasPlaced" @click="validatePlacement" :disabled="disableSubmitPlacement"
            label="Valider placement" />
        <WaitingButton v-else :label="labelWaitingForOpponent" />

        <GameBoardShipToolBox />
    </div>
</template>
