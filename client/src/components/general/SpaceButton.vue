<script setup lang="ts">
import HologramText from './HologramText.vue'
import SpaceContainer from './SpaceContainer.vue'

const propsComponent = withDefaults(
  defineProps<{
    label: string
    disabled?: boolean
    color?: 'primary' | 'secondary'
    size?: 'sm' | 'md'
  }>(),
  {
    label: '',
    disabled: false,
    color: 'primary',
    size: 'md',
  },
)
</script>

<template>
  <SpaceContainer
    :color="propsComponent.color"
    :disabled="propsComponent.disabled"
    :size="propsComponent.size"
    highlitable="hover"
    scan-line="hover"
    class="flex flex-center"
  >
    <HologramText :color="propsComponent.color" :text="propsComponent.label" />
  </SpaceContainer>
</template>
