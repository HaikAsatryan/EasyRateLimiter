import http from 'k6/http';
import { check, sleep } from 'k6';

const API_BASE_URL = "http://localhost";

const API_ENDPOINT_SYNC = "/above-board/ping";
//const API_ENDPOINT_SYNC2 = "/above-board/headers";



export let options = {
  vus: 100,
  duration: '20s',

  summaryTrendStats: ['avg', 'min', 'max'],
  summaryTimeUnit: 'ms'
  };


export default function () {
    const res = http.get(API_BASE_URL+API_ENDPOINT_SYNC);
    check(res, {
        'status was 200': (r) => r.status == 200,
        'status was 429': (r) => r.status == 429
    });

    sleep(0.1); // Simulate wait time between requests
}
