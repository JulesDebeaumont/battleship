<script setup lang="ts">
import type { IShipPlacement } from 'src/stores/room-placement-store'
import {
  BIG_SHIP_SIZE,
  MEDIUM_SHIP_SIZE,
  SMALL_SHIP_SIZE,
  SQUARE_SIZE,
  SQUARE_SIZE_UNIT,
} from 'src/utils/gameboard-boxes'
import { computed } from 'vue'

const propsComponents = withDefaults(
  defineProps<{
    shipPlacement: IShipPlacement
    toolbox?: boolean
  }>(),
  {
    toolbox: false,
  },
)

const classesContainer = computed(() => {
  let classes = ''
  if (propsComponents.toolbox) classes += ' ship-toolbox'
  else classes += ' ship-container'
  if (propsComponents.toolbox && !propsComponents.shipPlacement.enabled)
    classes += ' ship-toolbox-disabled'
  return classes
})
const stylesContainer = computed(() => {
  let styles = ''
  let size = 0
  if (propsComponents.shipPlacement.type === 'big') size = BIG_SHIP_SIZE
  if (propsComponents.shipPlacement.type === 'medium') size = MEDIUM_SHIP_SIZE
  if (propsComponents.shipPlacement.type === 'small') size = SMALL_SHIP_SIZE
  if (propsComponents.shipPlacement.orientation === 'horizontal')
    styles += ` width:${size}${SQUARE_SIZE_UNIT}; height: ${SQUARE_SIZE}${SQUARE_SIZE_UNIT};`
  if (propsComponents.shipPlacement.orientation === 'vertical')
    styles += ` width:${SQUARE_SIZE}${SQUARE_SIZE_UNIT}; height: ${size}${SQUARE_SIZE_UNIT};`
  return styles
})
const classesShip = computed(() => {
  let classes = 'ship'
  if (propsComponents.shipPlacement.orientation === 'horizontal') classes += ' ship-horizontal'
  if (propsComponents.shipPlacement.orientation === 'vertical') classes += ' ship-vertical'
  if (propsComponents.shipPlacement.type === 'big') classes += ' ship-big'
  if (propsComponents.shipPlacement.type === 'medium') classes += ' ship-medium'
  if (propsComponents.shipPlacement.type === 'small') classes += ' ship-small'
  return classes
})
const stylesShip = computed(() => {
  let styles = ''
  let size = 0
  if (propsComponents.shipPlacement.type === 'big') size = BIG_SHIP_SIZE
  if (propsComponents.shipPlacement.type === 'medium') size = MEDIUM_SHIP_SIZE
  if (propsComponents.shipPlacement.type === 'small') size = SMALL_SHIP_SIZE
  styles += ` width:${SQUARE_SIZE}${SQUARE_SIZE_UNIT}; height: ${size}${SQUARE_SIZE_UNIT};`
  return styles
})
</script>

<template>
  <div :class="classesContainer" :style="stylesContainer">
    <div :class="classesShip" :style="stylesShip"></div>
  </div>
</template>

<style>
.ship-container {
  overflow: visible;
  position: absolute;
  z-index: 2;
}

.ship-toolbox {
  overflow: visible;
  height: 100%;
  width: 100%;
  position: relative;
}

.ship-toolbox-disabled {
  filter: grayscale(80%) brightness(40%);
}
.ship {
  display: flex;
  background-size: 100% 100%;
}
.ship-vertical {
  transform: rotate(0deg);
}

.ship-horizontal {
  position: absolute;
  top: 0;
  left: 100%;
  transform: rotate(90deg);
  transform-origin: top left;
}

.ship-small {
  background-image: url('/images/ship-small.png');
}

.ship-medium {
  background-image: url('/images/ship-medium.png');
}

.ship-big {
  background-image: url('/images/ship-big.png');
}
</style>
