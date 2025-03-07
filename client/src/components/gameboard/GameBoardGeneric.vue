<script setup lang="ts">
import { GRID_SIZE } from 'src/stores/room-opponent-store';
import GameBoardIndicatorBox from './boxes/GameBoardIndicatorBox.vue';
</script>

<template>
     <div class="flex">
        <div v-for="row in GRID_SIZE" :key="row" class="flex column">
            <div v-for="cell in GRID_SIZE" :key="cell" class="flex row no wrap">
                <GameBoardIndicatorBox v-if="row === 1 && cell === 1" :display="''" />
                <GameBoardIndicatorBox v-if="row === 1 && cell > 1" :display="String.fromCharCode(cell + 63)" />
                <GameBoardIndicatorBox v-if="row > 1 && cell === 1" :display="row - 1" />
                <slot v-if="row > 1 && cell > 1" name="boxes" :xOffset="row - 2" :yOffset="cell - 2"></slot>
            </div>
        </div>
    </div>
</template>