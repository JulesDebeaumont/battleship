<script setup lang="ts">
import { computed } from 'vue'

const propsComponent = withDefaults(
  defineProps<{
    color?: 'primary' | 'secondary'
    size?: 'sm' | 'md'
  }>(),
  {
    color: 'primary',
    size: 'md',
  },
)

const classesRoot = computed(() => {
  return `space-card space-card-${propsComponent.color} space-card-${propsComponent.size}`
})
</script>

<template>
  <div :class="classesRoot">
    <slot></slot>
  </div>
</template>

<style>
.space-card {
  display: flex;
  flex-direction: column;
  position: relative;
  color: #fff;
  overflow: hidden;
  transition: all 0.1s ease;
  backdrop-filter: blur(5px);
}
.space-card-sm {
  padding: 1rem 1rem;
  font-size: 1.1rem;
  font-weight: 600;
  margin: 7px;
}
.space-card-md {
  padding: 1.3rem 3rem;
  font-size: 1.1rem;
  font-weight: 600;
  margin: 20px;
}
.space-card-primary {
  border: 2px solid rgba(0, 255, 255, 0.2);
  background: rgb(0, 39, 39);
  box-shadow: 0 0 15px rgb(0, 255, 255, 0.3);
}
.space-card-secondary {
  border: 2px solid rgba(0, 225, 255, 0.2);
  background: rgb(6, 27, 39);
  box-shadow: 0 0 15px rgba(0, 136, 255, 0.3);
}
</style>
