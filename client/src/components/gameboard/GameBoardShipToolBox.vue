<script setup lang="ts">
import type { IShipPlacement } from 'src/stores/room-placement-store';
import { useRoomPlacementStore } from 'src/stores/room-placement-store';
import draggableComponent from 'vuedraggable';

const placementStore = useRoomPlacementStore()

function cloneItem(item: IShipPlacement) {
    const itemClone = JSON.parse(JSON.stringify(item));
    return itemClone;
}
</script>

<template>
    <q-card class="q-pa-lg flex row q-gutter-sm">
        <div class="flex row q-gutter-sm">
            <template v-for="ship in placementStore.verticalBoatList" :key="ship.guid">
                <template v-if="ship.enabled">
                    <draggableComponent :list="[ship]" :group="{ name: 'toolbox', pull: 'clone', put: false }"
                        :clone="cloneItem" item-key="guid" :sort="false">
                        <template #item="{ element }">
                            <div class="flex">
                                <div class="ship-toolbox" :class="element.classes">
                                </div>
                            </div>
                        </template>
                    </draggableComponent>
                </template>
                <template v-else>
                    <div class="flex">
                        <div class="ship-toolbox-disabled" :class="ship.classes">
                        </div>
                    </div>
                </template>
            </template>
        </div>

        <div class="flex column q-gutter-sm">
            <template v-for="ship in placementStore.horizontalBoatList" :key="ship.guid">
                <template v-if="ship.enabled">
                    <draggableComponent :list="[ship]" :group="{ name: 'toolbox', pull: 'clone', put: false }"
                        :clone="cloneItem" item-key="guid" :sort="false">
                        <template #item="{ element }">
                            <div class="flex">
                                <div class="ship-toolbox" :class="element.classes">
                                </div>
                            </div>
                        </template>
                    </draggableComponent>
                </template>
                <template v-else>
                    <div class="flex">
                        <div class="ship-toolbox-disabled" :class="ship.classes">
                        </div>
                    </div>
                </template>
            </template>
        </div>
        
    </q-card>
</template>
