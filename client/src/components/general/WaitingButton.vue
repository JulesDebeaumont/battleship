<script setup lang="ts">
import HologramText from './HologramText.vue'
import SpaceContainer from './SpaceContainer.vue'

const propsComponent = withDefaults(
  defineProps<{
    label: string
    color?: 'primary' | 'secondary'
    size?: 'sm' | 'md'
  }>(),
  {
    label: '',
    color: 'primary',
    size: 'md',
  },
)
const propsEmits = defineEmits<{
  (e: 'click'): Promise<void> | void
}>()
</script>

<template>
  <SpaceContainer
    @click="propsEmits('click')"
    :color="propsComponent.color"
    :disabled="false"
    :size="propsComponent.size"
    highlitable="always"
    scan-line="always"
    class="space-button-waiting"
  >
    <HologramText color="primary" :text="propsComponent.label" />
  </SpaceContainer>
</template>

<style>
.space-button-waiting {
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
