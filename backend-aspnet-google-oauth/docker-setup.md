# Docker Setup Guide for Google OAuth Backend (PostgreSQL)

This guide will help you run the ASP.NET Core Web API with Google OAuth using Docker and Docker Compose with PostgreSQL database.

## Prerequisites

1. **Docker Desktop** installed and running
2. **Docker Compose** (included with Docker Desktop)
3. **Google OAuth credentials** (already configured)

## Quick Start

### 1. Development Environment

```bash
# Clone or navigate to your project directory
cd backend-aspnet-google-oauth

# Update configuration with your Google credentials
# Edit appsettings.Development.json with your actual values

# Build and run all services
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build
```

### 2. Production Environment

```bash
# Copy environment example
cp env.example .env

# Edit .env with your production values
# Update Google credentials, JWT secrets, and database password

# Run production stack
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

## Available Commands

### Development Commands

```bash
# Start all services
docker-compose up

# Start in background
docker-compose up -d

# Rebuild and start
docker-compose up --build

# View logs
docker-compose logs

# View logs for specific service
docker-compose logs api
docker-compose logs postgres

# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: This will delete database data)
docker-compose down -v
```

### Production Commands

```bash
# Start production stack
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# View production logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs

# Stop production stack
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
```

### Database Commands

```bash
# Run Entity Framework migrations
docker-compose exec api dotnet ef database update

# Create new migration
docker-compose exec api dotnet ef migrations add MigrationName

# Remove last migration
docker-compose exec api dotnet ef migrations remove

# Access PostgreSQL directly
docker-compose exec postgres psql -U postgres -d YourApiDb

# Backup database
docker-compose exec postgres pg_dump -U postgres YourApiDb > backup.sql
```

### Maintenance Commands

```bash
# View running containers
docker ps

# View container details
docker inspect google-oauth-api

# Access container shell
docker-compose exec api sh

# Access PostgreSQL shell
docker-compose exec postgres psql -U postgres -d YourApiDb

# Backup database
docker-compose exec postgres pg_dump -U postgres YourApiDb > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres -d YourApiDb < backup.sql
```

## Configuration

### Environment Variables

Create a `.env` file in the project root:

```bash
# Copy example
cp env.example .env

# Edit with your values
nano .env
```

Required environment variables:

```bash
# Database
DB_PASSWORD=YourStrong@Passw0rd
POSTGRES_DB=YourApiDb
POSTGRES_USER=postgres

# Google OAuth
GOOGLE_CLIENT_ID=your-actual-google-client-id
GOOGLE_CLIENT_SECRET=your-actual-google-client-secret
GOOGLE_REDIRECT_URI=http://localhost:7002/api/UserLogin/auth/google/callback

# JWT
JWT_SECRET_KEY=your-super-secret-jwt-key-here-make-it-long-and-secure
JWT_ISSUER=your-api-issuer
JWT_AUDIENCE=your-api-audience
JWT_EXPIRATION_MINUTES=60
```

### Port Configuration

Default ports:
- **API**: `http://localhost:7002`
- **PostgreSQL**: `localhost:5455`
- **pgAdmin** (optional): `http://localhost:8080`

To change ports, edit `docker-compose.yml`:

```yaml
services:
  api:
    ports:
      - "YOUR_PORT:80"  # Change YOUR_PORT to desired port
```

## Services Overview

### 1. PostgreSQL Database
- **Image**: `postgres:latest`
- **Port**: 5455
- **Database**: YourApiDb
- **User**: postgres
- **Password**: postgres (development) / configurable (production)

### 2. ASP.NET Core API (GoogleLoginApi)
- **Port**: 7002
- **Environment**: Development/Production
- **Features**: Google OAuth, JWT, Entity Framework with PostgreSQL

### 3. pgAdmin (Optional)
- **Port**: 8080
- **Purpose**: Web-based PostgreSQL client
- **Usage**: Uncomment in docker-compose.yml
- **Login**: admin@admin.com / admin

## Troubleshooting

### Common Issues

#### 1. Port Already in Use
```bash
# Check what's using the port
netstat -ano | findstr :7002
netstat -ano | findstr :5455

# Kill the process or change port in docker-compose.yml
```

#### 2. Database Connection Issues
```bash
# Check if PostgreSQL is running
docker-compose ps

# Check PostgreSQL logs
docker-compose logs postgres

# Wait for PostgreSQL to be ready
docker-compose logs -f postgres

# Test connection
docker-compose exec postgres pg_isready -U postgres -d YourApiDb
```

#### 3. API Build Failures
```bash
# Clean Docker cache
docker system prune -a

# Rebuild without cache
docker-compose build --no-cache
```

#### 4. Permission Issues
```bash
# On Linux/Mac, ensure proper file permissions
chmod 755 .

# On Windows, run Docker Desktop as Administrator
```

#### 5. PostgreSQL Specific Issues
```bash
# Check PostgreSQL status
docker-compose exec postgres pg_isready

# View PostgreSQL logs
docker-compose logs postgres

# Reset PostgreSQL data (WARNING: This will delete all data)
docker-compose down -v
docker-compose up --build
```

### Health Checks

```bash
# Check API health
curl http://localhost:7002/health

# Check PostgreSQL health
docker-compose exec postgres pg_isready -U postgres -d YourApiDb

# Test database connection
docker-compose exec postgres psql -U postgres -d YourApiDb -c "SELECT 1;"
```

### Logs and Debugging

```bash
# View all logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View specific service logs
docker-compose logs -f api
docker-compose logs -f postgres

# View last 100 lines
docker-compose logs --tail=100 api
```

## Development Workflow

### 1. First Time Setup
```bash
# Clone project
git clone <your-repo>
cd backend-aspnet-google-oauth

# Configure environment
cp env.example .env
# Edit .env with your credentials

# Start services
docker-compose up --build

# Run migrations
docker-compose exec api dotnet ef database update
```

### 2. Daily Development
```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f api

# Make code changes
# Rebuild when needed
docker-compose up --build api
```

### 3. Testing
```bash
# Test API endpoints
curl http://localhost:7002/swagger

# Test Google OAuth
curl http://localhost:7002/api/UserLogin/auth/google

# Test database connection
docker-compose exec postgres psql -U postgres -d YourApiDb -c "SELECT COUNT(*) FROM \"Users\";"
```

## Production Deployment

### 1. Environment Setup
```bash
# Create production environment file
cp env.example .env.prod

# Edit with production values
nano .env.prod

# Load production environment
export $(cat .env.prod | xargs)
```

### 2. Deploy
```bash
# Deploy production stack
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build

# Verify deployment
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps
```

### 3. Monitoring
```bash
# Monitor logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f

# Check resource usage
docker stats

# Monitor database
docker-compose exec postgres psql -U postgres -d YourApiDb -c "SELECT * FROM pg_stat_activity;"
```

## Database Management

### PostgreSQL Commands

```bash
# Connect to database
docker-compose exec postgres psql -U postgres -d YourApiDb

# List tables
\dt

# Describe table
\d "Users"

# View data
SELECT * FROM "Users";

# Exit PostgreSQL
\q
```

### Backup and Restore

```bash
# Create backup
docker-compose exec postgres pg_dump -U postgres YourApiDb > backup_$(date +%Y%m%d_%H%M%S).sql

# Restore from backup
docker-compose exec -T postgres psql -U postgres -d YourApiDb < backup.sql

# Backup with compression
docker-compose exec postgres pg_dump -U postgres YourApiDb | gzip > backup.sql.gz

# Restore from compressed backup
gunzip -c backup.sql.gz | docker-compose exec -T postgres psql -U postgres -d YourApiDb
```

## Security Considerations

1. **Change default passwords** in production
2. **Use strong JWT secrets**
3. **Enable HTTPS** in production
4. **Restrict network access**
5. **Regular security updates**
6. **Backup database regularly**
7. **Use PostgreSQL authentication** properly
8. **Enable SSL** for database connections in production

## Performance Optimization

### Development
- Use volume mounts for hot reload
- Enable detailed logging
- Use development configuration

### Production
- Use resource limits
- Enable compression
- Use production configuration
- Monitor resource usage
- Implement caching strategies
- Optimize PostgreSQL settings
- Use connection pooling 