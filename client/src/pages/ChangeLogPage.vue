<script setup lang="ts">
import HologramText from 'src/components/general/HologramText.vue'
import SpaceButton from 'src/components/general/SpaceButton.vue'
import SpaceContainer from 'src/components/general/SpaceContainer.vue'
import { allLogs } from 'src/utils/changelog'
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="flex items-center column">
    <SpaceButton size="sm" label="Retour" @click="router.push({ name: 'me' })" />
    <SpaceContainer
      v-for="log in allLogs"
      :key="log.date"
      :disabled="false"
      highlitable="hover"
      scan-line="hover"
    >
      <HologramText :text="`${log.title} ${log.date}`" class="text-h4" />

      <div class="q-py-md">
        <div v-for="core in log.cores" :key="core.type">
          <div>
            <span class="text-h6 text-weight-medium text-cyan-8"
              >{{ core.type === 'changes' ? 'Changements' : 'Correctifs' }}
            </span>
          </div>
          <div class="q-pb-lg q-pt-sm no-margin">
            <div v-for="logFragment in core.list" :key="logFragment.text">
              <div class="full-width flex row">
                <div class="text-body2 log-text full-width">- {{ logFragment.text }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <span class="text-grey-7 text-body2"
        ><i>{{ log.note }}</i></span
      >
    </SpaceContainer>
  </div>
</template>
