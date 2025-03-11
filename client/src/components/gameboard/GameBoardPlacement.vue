<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import GameBoardGeneric from './GameBoardGeneric.vue';
import GameBoardPlacementBox from './boxes/GameBoardPlacementBox.vue';
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoomPlacementStore } from 'src/stores/room-placement-store';
import GameBoardShipToolBox from './GameBoardShipToolBox.vue';
import { useRoute } from 'vue-router';
import SpaceButton from '../general/SpaceButton.vue';
import WaitingButton from '../general/WaitingButton.vue';

const roomStore = useRoomOpponentStore()
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

onMounted(() => {
    placementStore.setup(roomGuid)
})
onUnmounted(() => {
    placementStore.clear()
})
</script>

<template>
    <div class="flex flex-center column">
        <GameBoardGeneric class="q-mb-md">
            <template v-slot:boxes="boxSlot">
                <GameBoardPlacementBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>

        <SpaceButton v-if="!hasPlaced" @click="validatePlacement" :disabled="disableSubmitPlacement" label="Valider placement" />
        <WaitingButton v-else :label="labelWaitingForOpponent" />

        <GameBoardShipToolBox v-if="!hasPlaced" />
    </div>
</template>
