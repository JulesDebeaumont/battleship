<script setup lang="ts">
import type { IShipPlacement } from 'src/stores/room-placement-store';
import { useRoomPlacementStore } from 'src/stores/room-placement-store';
import { ref, watch } from 'vue';
import draggableComponent from 'vuedraggable';

const propsComponents = defineProps<{
    xOffset: number
    yOffset: number
}>()

const placementStore = useRoomPlacementStore()

const listPlacement = ref<IShipPlacement[]>([])
const isFrozen = ref<boolean>(false)

function onDrop() {
    if (listPlacement.value.length > 1) listPlacement.value.shift()
    if (listPlacement.value.length === 0) return
    const canAdd = placementStore.tryAddToCurrentPlacement(listPlacement.value.at(0)!, propsComponents.xOffset, propsComponents.yOffset)
    if (!canAdd) {
        listPlacement.value = []
    }
}

watch(() => placementStore.hasSubmitPlacement, (newValue) => {
    if (newValue) isFrozen.value = newValue
})
</script>

<template>
    <div class="gameboard-box">
        <div style="position: absolute;">{{ propsComponents.xOffset }} / {{ propsComponents.yOffset }}</div>
        <template v-if="!isFrozen">
            <draggableComponent @add="onDrop" :list="listPlacement"
                :group="{ name: `gameboard-boxes-${propsComponents.xOffset}-${propsComponents.yOffset}`, put: true }"
                item-key="guid" class="full-width full-height" :sort="false">
                <template #item="{ element }">
                    <div class="ship" :class="element.classes">
                    </div>
                </template>
            </draggableComponent>
        </template>
        <template v-else>
            <div class="ship" :class="listPlacement.at(0)?.classes">
            </div>
        </template>
    </div>
</template>
