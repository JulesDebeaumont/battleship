<script setup lang="ts">
import type { IShipPlacement } from 'src/stores/room-placement-store'
import { useRoomPlacementStore } from 'src/stores/room-placement-store'
import { ref, watch } from 'vue'
import draggableComponent from 'vuedraggable'
import Ship from '../ShipComponent.vue'

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
  const canAdd = placementStore.tryAddToCurrentPlacement(
    listPlacement.value.at(0)!,
    propsComponents.xOffset,
    propsComponents.yOffset,
  )
  if (!canAdd) {
    listPlacement.value = []
  }
}

watch(
  () => placementStore.hasSubmitPlacement,
  (newValue) => {
    if (newValue) isFrozen.value = newValue
  },
)
</script>

<template>
  <div class="gameboard-box gameboard-box-placement">
    <template v-if="!isFrozen">
      <draggableComponent
        @add="onDrop"
        :list="listPlacement"
        :group="{
          name: `gameboard-boxes-${propsComponents.xOffset}-${propsComponents.yOffset}`,
          put: true,
        }"
        item-key="guid"
        class="full-width full-height draggable-ship"
        :sort="false"
      >
        <template #item="{ element }">
          <Ship :ship-placement="element" />
        </template>
      </draggableComponent>
    </template>
    <template v-else>
      <Ship v-if="listPlacement.at(0)" :ship-placement="listPlacement.at(0)!" />
    </template>
  </div>
</template>

<style>
.gameboard-box-placement {
  background-color: #5a8f8fb0;
  transition: all 0.1s;
}
.gameboard-box-placement:hover {
  background-color: #5a8f8fe7;
}
</style>
