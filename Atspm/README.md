# ATSPM Docker Compose Setup

This project uses Docker Compose to orchestrate multiple services required for running the ATSPM (Automated Traffic Signal Performance Measures) application stack, including APIs, database, and frontend.

## Prerequisites

- Docker
- Docker Compose
- OpenSSL

## Certificate Setup

Each developer must generate SSL certificate files using OpenSSL. These files are required for HTTPS support and must be placed in the `nginx/certs` folder.

### Steps to Generate Certificates (Windows PowerShell)

1. Open PowerShell as Administrator.
2. Navigate to the `ATSPM` folder:
   ```powershell
   cd path\to\ATSPM
   ```
3. Run the following commands to create the certs folder and generate the certificates:
   ```powershell
   mkdir nginx\certs
   openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout nginx\certs\aspnetapp.key -out nginx\certs\aspnetapp.crt -subj "/CN=localhost"
   openssl pkcs12 -export -out nginx\certs\aspnetapp.pfx -inkey nginx\certs\aspnetapp.key -in nginx\certs\aspnetapp.crt -passout pass:password
   ```
4. These files are now used by the Docker services via a volume mount defined in the `.env` file:
   ```env
   CERT_LOCATION=./nginx/certs:/root/.aspnet/https:ro
   ```

## Environment Variables

Create a `.env` file in the root of the project (next to `docker-compose.yml`). This file **must not** be checked in to version control due to sensitive data.

### Sample `.env` File

```env
# Database Credentials
POSTGRES_USER=admin
POSTGRES_PASSWORD=admin

# Connection Strings
CONFIG_CONNECTION=Host=postgres;Port=5432;Username=admin;Password=admin;Database=ATSPM-Config;Pooling=true; Timeout=30;CommandTimeout=60;
AGGREGATION_CONNECTION=Host=postgres;Port=5432;Username=admin;Password=admin;Database=ATSPM-Aggregation;Pooling=true; Timeout=30;CommandTimeout=60;
EVENTLOG_CONNECTION=Host=postgres;Port=5432;Username=admin;Password=admin;Database=ATSPM-EventLogs;Pooling=true; Timeout=30;CommandTimeout=60;
IDENTITY_CONNECTION=Host=postgres;Port=5432;Username=admin;Password=admin;Database=ATSPM-Identity;Pooling=true; Timeout=30;CommandTimeout=60;

# Database Providers
ConfigContext_Provider=PostgreSQL
AggregationContext_Provider=PostgreSQL
EventLogContext_Provider=PostgreSQL
IdentityContext_Provider=PostgreSQL

# Admin Configuration
ADMIN_EMAIL=dlowe@avenueconsultants.com
ADMIN_ROLE=Admin
ADMIN_PASSWORD="ThisIsAPassword1!"
SEED_ADMIN=true

# Allowed Hosts
ALLOWED_HOSTS=*

# Email Configuration
EmailConfiguration__Host=smtp.freesmtpservers.com
EmailConfiguration__Port=25
EmailConfiguration__UserName=
EmailConfiguration__Password=
EmailConfiguration__EnableSsl=false

# JWT Configuration
JWT_EXPIRE_DAYS=1
JWT_KEY=ATSPMProductionIdentityOpenSource2024
JWT_ISSUER=AvenueConsultants

# Certificate Volume Mapping
CERT_LOCATION=./nginx/certs:/root/.aspnet/https:ro

# Watchdog Configuration
WatchdogConfiguration__ScanDate=2025-01-15
WatchdogConfiguration__ConsecutiveCount=3
WatchdogConfiguration__LowHitThreshold=50
WatchdogConfiguration__MaximumPedestrianEvents=200
WatchdogConfiguration__MinimumRecords=500
WatchdogConfiguration__MinPhaseTerminations=50
WatchdogConfiguration__PercentThreshold=0.9
WatchdogConfiguration__PreviousDayPMPeakEnd=18
WatchdogConfiguration__PreviousDayPMPeakStart=17
WatchdogConfiguration__ScanDayEndHour=5
WatchdogConfiguration__ScanDayStartHour=1
WatchdogConfiguration__RampMainlineStartHour=15
WatchdogConfiguration__RampMainlineEndHour=19
WatchdogConfiguration__RampStuckQueueStartHour=1
WatchdogConfiguration__RampStuckQueueEndHour=4
WatchdogConfiguration__WeekdayOnly=false
WatchdogConfiguration__DefaultEmailAddress=dlowe@avenueconsultants.com
WatchdogConfiguration__EmailAllErrors=false
WatchdogConfiguration__Sort=Location
```

## Running the Project

To spin up the stack:
```bash
docker-compose up --build
```

Make sure the `.env` file is present and the certificates are generated before running the above command.

---

For issues or help, reach out to your DevOps or development lead.

