<script setup lang="ts">
import { computed } from 'vue'

const propsComponent = withDefaults(
  defineProps<{
    color?: 'primary' | 'secondary'
    highlitable: 'hover' | 'always' | 'none'
    scanLine: 'hover' | 'always' | 'none'
    disabled?: boolean
    size?: 'sm' | 'md'
  }>(),
  {
    color: 'primary',
    disabled: false,
    size: 'md',
  },
)

const classesRoot = computed(() => {
  let classes = `space-container space-container-${propsComponent.color} space-container-${propsComponent.size}`
  if (propsComponent.disabled) return (classes += ' space-container-disabled')
  if (propsComponent.highlitable === 'hover')
    classes += ` space-container-hoverable-highlight space-container-hoverable-highlight-${propsComponent.color}`
  if (propsComponent.highlitable === 'always')
    classes += ` space-container-highlight-${propsComponent.color}`
  if (propsComponent.scanLine === 'hover') classes += ' space-container-hoverable-scan-line'
  return classes
})
const classesScanLine = computed(() => {
  let classes = `scan-line scan-line-${propsComponent.color}`
  if (propsComponent.scanLine === 'always') classes += ' scan-line-displayed'
  return classes
})
</script>

<template>
  <div :class="classesRoot">
    <div v-if="propsComponent.scanLine !== 'none'" :class="classesScanLine"></div>
    <slot></slot>
  </div>
</template>

<style>
.space-container {
  display: flex;
  flex-direction: column;
  position: relative;
  color: #fff;
  overflow: hidden;
  transition: all 0.1s ease;
  backdrop-filter: blur(5px);
  transition: all 0.1s;
}
.space-container-sm {
  padding: 1rem 1rem;
  font-size: 1.1rem;
  font-weight: 600;
  margin: 7px;
}
.space-container-md {
  padding: 1.3rem 3rem;
  font-size: 1.1rem;
  font-weight: 600;
  margin: 20px;
}
.space-container-primary {
  border: 2px solid rgba(0, 255, 255, 0.5);
  background: rgba(0, 255, 255, 0.1);
  box-shadow: 0 0 15px rgba(0, 255, 255, 0.3);
}
.space-container-secondary {
  border: 2px solid rgba(0, 225, 255, 0.5);
  background: rgba(0, 166, 255, 0.1);
  box-shadow: 0 0 15px rgba(0, 136, 255, 0.3);
}
.space-container-disabled {
  filter: grayscale(50%) brightness(80%);
}
.space-container-hoverable-scan-line:hover .scan-line {
  opacity: 100% !important;
}
.space-container-disabled:hover {
  cursor: default !important;
}
.space-container-disabled:hover .scan-line {
  opacity: 0% !important;
}
.space-container-hoverable-highlight:hover {
  cursor: pointer;
}
.space-container-hoverable-highlight-primary:hover,
.space-container-highlight-primary {
  background: rgba(0, 255, 255, 0.233);
}
.space-container-hoverable-highlight-secondary:hover,
.space-container-highlight-secondary {
  background: rgba(0, 179, 255, 0.233);
}

.space-container-highlight {
  cursor: default;
}
.scan-line {
  opacity: 0%;
  position: absolute;
  width: 100%;
  height: 2px;
  top: 0;
  animation: scan 2s linear infinite;
  transition: all 0.1s;
  filter: blur(1px);
}
.scan-line-primary {
  background: linear-gradient(to right, transparent, rgba(0, 255, 255, 0.8), transparent);
}
.scan-line-secondary {
  background: linear-gradient(to right, transparent, rgba(0, 179, 255, 0.8), transparent);
}
.scan-line-displayed {
  opacity: 100%;
}

@keyframes scan {
  0% {
    top: -10%;
  }

  100% {
    top: 110%;
  }
}
</style>
