// #region license
// Copyright 2024 Utah Department of Transportation
// for WebUI - charts.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
import { http, HttpResponse } from 'msw'
import { rampMeteringData } from '../data/charts/rampMeteringData'

export const rampMeteringHandler = [
  http.post(
    'https://report-api-bdppc3riba-wm.a.run.app/v1/RampMetering/GetReportData',
    () => {
      return HttpResponse.json(rampMeteringData)
    }
  ),
]
