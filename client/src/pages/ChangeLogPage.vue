<script setup lang="ts">
import LogoAlert from 'src/components/changelog/LogoAlert.vue';
import LogoChanges from 'src/components/changelog/LogoChanges.vue';
import LogoFixes from 'src/components/changelog/LogoFixes.vue';
import LogoNew from 'src/components/changelog/LogoNew.vue';
import type { ILogComponentInfo } from 'src/utils/changelog'
import { allLogs } from 'src/utils/changelog';
import { h } from 'vue';

function getTitleLogo(type: ILogComponentInfo['cores'][number]['type']) {
  if (type === 'changes') return LogoChanges
  if (type === 'fixes') return LogoFixes
  return h
}
function getLogoFragment(iconName: ILogComponentInfo['cores'][number]['list'][number]['icon']) {
  if (iconName === 'new') return LogoNew
  if (iconName === 'alert') return LogoAlert
  return h
}
</script>

<template>
  <div class="flex column">

    <div class="flex items-center column q-pt-xl">
      <div v-for="log in allLogs" :key="log.date" :log="log" :id="log.date.replaceAll('/', '')"
        class="q-pa-lg q-mb-xl bg-blue-grey-1 rounded shadow-5 log">
        <span class="flex text-h4 text-cyan-8 text-weight-bold flex-center">{{ log.title }} - <i>{{ log.date
        }}</i>
        </span>

        <div class="q-py-md">
          <div v-for="core in log.cores" :key="core.type">
            <div>
              <span class="text-h6 text-weight-medium text-cyan-8">{{
                core.type
              }}
                <component :is="getTitleLogo(core.type)" class="logo-title" />
              </span>
            </div>
            <ul class="q-pb-lg q-pt-sm no-margin">
              <li v-for="logFragment in core.list" :key="logFragment.text">
                <div :class="`full-width flex row priority${logFragment.priority ?? ''
                  }`">
                  <div class="text-body2 log-text full-width">
                    ● {{ logFragment.text }}
                    <component v-if="logFragment.icon" :is="getLogoFragment(logFragment.icon)" class="logo-log" />
                  </div>
                </div>
              </li>
            </ul>
          </div>
        </div>

        <span class="text-grey-7 text-body2"><i>{{ log.note }}</i></span>
      </div>
    </div>
  </div>
</template>

<style scoped>
ul,
li {
  list-style: none;
}

.priority1:hover {
  background-color: #dbe7ec;
  transition: 0.2s;
}

.priority:hover {
  background-color: #e5eee3;
  transition: 0.2s;
}

.priority2:hover {
  background-color: #eeead8;
  transition: 0.2s;
}

.priority3:hover {
  background-color: #f1e6e6;
  transition: 0.2s;
}

.log {
  width: 50%;
}

@media screen and (max-width: 1600px) {
  .log {
    width: 60%;
  }
}

@media screen and (max-width: 500px) {
  .log {
    width: 80%;
  }
}

.logo-title {
  position: relative;
  top: +6px;
}

.logo-log {
  position: relative;
  top: +6px;
}
</style>
