<script setup lang="ts">
const propsComponent = withDefaults(
  defineProps<{
    text: string
    color?: 'primary' | 'secondary'
  }>(),
  {
    color: 'primary',
  },
)
</script>

<template>
  <span
    :class="`hologram-text hologram-${propsComponent.color}`"
    :data-text="propsComponent.text"
    >{{ propsComponent.text }}</span
  >
</template>

<style>
.hologram-text {
  position: relative;
  display: inline-block;
}
.hologram-primary {
  text-shadow: 0 0 8px rgba(0, 255, 255, 0.5);
}
.hologram-secondary {
  text-shadow: 0 0 8px rgba(0, 60, 255, 0.5);
}
.hologram-text::after,
.hologram-text::before {
  content: attr(data-text);
  position: absolute;
  left: 0;
  opacity: 0;
  filter: blur(1px);
  transition: all 0.3s ease;
}
.hologram-text::before {
  top: -2px;
  transform: translateX(0);
  animation: glitch 2s infinite;
}
.hologram-primary::before {
  color: #ff00ff;
}
.hologram-secondary::before {
  color: #8000ff;
}
.hologram-text::after {
  bottom: -2px;
  transform: translateX(0);
  animation: glitch 2s infinite reverse;
}
.hologram-primary::after {
  color: #00ffff;
}
.hologram-secondary::before {
  color: #00a2ff;
}

@keyframes glitch {
  0%,
  100% {
    transform: translateX(0);
    opacity: 0.3;
  }

  20% {
    transform: translateX(-5px);
    opacity: 0.5;
  }

  40% {
    transform: translateX(5px);
    opacity: 0.7;
  }

  60% {
    transform: translateX(-3px);
    opacity: 0.5;
  }

  80% {
    transform: translateX(3px);
    opacity: 0.3;
  }
}
</style>
