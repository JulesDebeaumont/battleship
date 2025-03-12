<script setup lang="ts">
import type { IShipPlacement } from 'src/stores/room-placement-store'
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

const classes = computed(() => {
  let classes = ''
  if (propsComponents.toolbox) classes += ' ship-toolbox'
  if (!propsComponents.toolbox) classes += ' ship'
  if (propsComponents.toolbox && !propsComponents.shipPlacement.enabled)
    classes += ' ship-toolbox-disabled'
  if (propsComponents.shipPlacement.orientation === 'horizontal') classes += ' ship-horizontal'
  if (propsComponents.shipPlacement.orientation === 'vertical') classes += ' ship-vertical'
  if (propsComponents.shipPlacement.type === 'big') classes += ' ship-big'
  if (propsComponents.shipPlacement.type === 'medium') classes += ' ship-medium'
  if (propsComponents.shipPlacement.type === 'small') classes += ' ship-small'
  return classes
})

// TODO calculate height and width with JS + offsets data ? shiplacement.offsets * 60 px ?
</script>

<template>
  <div :class="classes"></div>
</template>

<style>
.ship {
  transform-origin: left top 0;
  white-space: nowrap;
  background-size: contain;
  overflow: visible;
  position: absolute;
  z-index: 2;
  height: 100%;
  width: 100%;
}
.ship-toolbox {
  background-size: 100% 100%;
  white-space: nowrap;
}
.ship-toolbox-disabled {
  filter: grayscale(80%);
}
.ship-small-horizontal {
  height: 60px;
  width: 180px;
}
.ship-small-vertical {
  height: 180px;
  width: 60px;
}
.ship-medium-horizontal {
}
.ship-medium-vertical {
}
.ship-big-horizontal {
}
.ship-big-vertical {
}
.ship-small {
  background-image: url('/images/ship-small.png');
  background-color: red;
}
.ship-medium {
  background-image: url('/images/ship-medium.png');
  background-color: blue;
  height: 240px;
  width: 60px;
}
.ship-big {
  background-image: url('/images/ship-big.png');
  background-color: green;
  height: 300px;
  width: 60px;
}
</style>
