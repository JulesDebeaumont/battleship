<script setup lang="ts">
import { computed } from 'vue';

const propsComponent = defineProps<{
    label: string
    disabled?: boolean
}>()
const propsEmits = defineEmits<{
    (e: 'click'): Promise<void> | void
}>()

const classesButton = computed(() => {
    let classes = 'space-btn'
    if (propsComponent.disabled) classes += ' space-btn-disabled'
    if (!propsComponent.disabled) classes += ' hologram'
    return classes
})
const classesScanLine = computed(() => {
    let classes = ''
    if (propsComponent.disabled) classes += ' scan-line-disabled'
    if (!propsComponent.disabled) classes += ' scan-line'
    return classes
})
</script>

<template>
    <button :class="classesButton" @click="propsEmits('click')">
        <span :data-text="propsComponent.label">{{ propsComponent.label }}</span>
        <div :class="classesScanLine"></div>
    </button>
</template>
