<script setup lang="ts">
import { useRoomFightStore } from 'src/stores/room-fight-store';
import GameBoardUserBox from './boxes/GameBoardUserBox.vue';
import GameBoardGeneric from './GameBoardGeneric.vue';
import SpaceButton from '../general/SpaceButton.vue';
const roomStore = useRoomFightStore()
</script>

<template>
    <div class="flex column flex-center">
        <SpaceButton :label="roomStore.getUserPseudo" size="sm" />
        <GameBoardGeneric class="q-mt-sm">
            <template v-slot:boxes="boxSlot">
                <GameBoardUserBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
