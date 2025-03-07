<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import GameBoardGeneric from './GameBoardGeneric.vue';
import GameBoardPlacementBox from './boxes/GameBoardPlacementBox.vue';
import { onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoomPlacementStore } from 'src/stores/room-placement-store';
import GameBoardShipToolBox from './GameBoardShipToolBox.vue';
import { useRoute } from 'vue-router';

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

onMounted(() => {
    placementStore.setup(roomGuid)
})
onUnmounted(() => {
    placementStore.clear()
})
</script>

<template>
    <div class="flex column">
        <GameBoardGeneric>
            <template v-slot:boxes="boxSlot">
                <GameBoardPlacementBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>

        <q-btn @click="validatePlacement" :disable="disableSubmitPlacement" label="Valider placement" />

        <GameBoardShipToolBox />
    </div>
</template>
