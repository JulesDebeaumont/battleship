<script setup lang="ts">
import type { IShipPlacement } from 'src/stores/room-placement-store'
import { useRoomPlacementStore } from 'src/stores/room-placement-store'
import draggableComponent from 'vuedraggable'
import SpaceCard from '../general/SpaceCard.vue'
import Ship from './ShipComponent.vue'

const placementStore = useRoomPlacementStore()

function cloneItem(item: IShipPlacement) {
  const itemClone = JSON.parse(JSON.stringify(item))
  return itemClone
}
</script>

<template>
  <SpaceCard>
    <div class="q-pa-lg flex row q-gutter-sm">
      <div class="flex row q-gutter-sm">
        <template v-for="ship in placementStore.verticalBoatList" :key="ship.guid">
          <template v-if="ship.enabled">
            <draggableComponent
              :list="[ship]"
              :group="{ name: 'toolbox', pull: 'clone', put: false }"
              :clone="cloneItem"
              :sort="false"
              item-key="guid"
              class="draggable-ship"
            >
              <template #item="{ element }">
                <Ship :ship-placement="element" :toolbox="true" />
              </template>
            </draggableComponent>
          </template>
          <template v-else>
            <Ship :ship-placement="ship" :toolbox="true" />
          </template>
        </template>
      </div>

      <div class="flex column q-gutter-sm">
        <template v-for="ship in placementStore.horizontalBoatList" :key="ship.guid">
          <template v-if="ship.enabled">
            <draggableComponent
              :list="[ship]"
              :group="{ name: 'toolbox', pull: 'clone', put: false }"
              :clone="cloneItem"
              :sort="false"
              item-key="guid"
              class="draggable-ship"
            >
              <template #item="{ element }">
                <Ship :ship-placement="element" :toolbox="true" />
              </template>
            </draggableComponent>
          </template>
          <template v-else>
            <Ship :ship-placement="ship" :toolbox="true" />
          </template>
        </template>
      </div>
    </div>
  </SpaceCard>
</template>
