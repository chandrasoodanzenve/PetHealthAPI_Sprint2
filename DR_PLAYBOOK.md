# 🚨 PetHealth Pro - Disaster Recovery (DR) Playbook

## 1. Objectives

- **RPO (Recovery Point Objective)**: < 15 minutes (Max data loss)
- **RTO (Recovery Time Objective)**: < 30 minutes (Max downtime)

## 2. Failover Procedure (Manual/Automated)

1. **Detection**: Traffic Manager health check flags Primary Region as "Down".
2. **Traffic Shift**: Front Door / Traffic Manager automatically routes 100% traffic to Secondary Region (West US).
3. **DB Promotion**: Trigger Azure SQL Failover Group to make the Secondary database the "Primary".

## 3. Post-Failover Checklist

- [ ] Verify API connectivity in West US.
- [ ] Check sync status of Redis Cache.
- [ ] Validate Audit logs are flowing to central storage.

## 4. Failback (Back to East US)

1. Ensure East US region is stable.
2. Re-sync data from West US to East US.
3. Switch Traffic Manager priority back to Primary.

## 📋 Emergency Incident Response Checklist

- [ ] **Triage**: Verify if the outage is regional or just a single service failure.
- [ ] **Communication**: Send "Service Disruption" notification to stakeholders.
- [ ] **Execution**: Manually trigger SQL Failover if "Automatic" mode is stuck.
- [ ] **Validation**: Run `curl /health` against the DR Global URL.
- [ ] **Reporting**: Document the incident in the Post-Mortem log.
