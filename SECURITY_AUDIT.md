#  PetHealth Pro - Kubernetes Security Audit Report

## 1. Role-Based Access Control (RBAC)

- **Status**:  Implemented
- **Audit**: Verified ServiceAccount `pethealth-sa` has 'Read-Only' access.
- **Command**: `kubectl auth can-i delete pods --as=system:serviceaccount:pethealth-prod:pethealth-sa`

## 2. Network Isolation

- **Status**:  Implemented
- **Audit**: Applied Egress NetworkPolicy. API Pod can ONLY communicate with the SQL Server pod.

## 3. Pod Hardening (Penetration Test Result)

- **Test**: Attempted to write to the root filesystem.
- **Result**: **REJECTED.** (ReadOnlyRootFilesystem: true).
- **Test**: Attempted to run container as Root user.
- **Result**: **REJECTED.** (runAsNonRoot: true).

## 4. Vulnerability Scanning

- **Status**:  Integrated in CI/CD via **Trivy**.
- **Rule**: Pipeline fails if High/Critical vulnerabilities are detected.

---
