<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import GameBoardUserBox from './boxes/GameBoardUserBox.vue';
import GameBoardGeneric from './GameBoardGeneric.vue';
import SpaceButton from '../general/SpaceButton.vue';
const roomStore = useRoomOpponentStore()
</script>

<template>
    <div class="flex q-pa-md column flex-center">
        <SpaceButton :label="roomStore.getUserPseudo" size="sm" />
        <GameBoardGeneric class="q-mt-sm">
            <template v-slot:boxes="boxSlot">
                <GameBoardUserBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
