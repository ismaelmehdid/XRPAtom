# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Install pnpm
RUN npm install -g pnpm

# Copy package files first
COPY ./src/package.json ./src/pnpm-lock.yaml ./

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy the rest of the application
COPY ./src/ .

# Build the application
RUN pnpm run build

# Production stage
FROM node:18-alpine AS runner

WORKDIR /app

# Install pnpm
RUN npm install -g pnpm

# Copy necessary files from build stage
COPY --from=build /app/package.json ./
COPY --from=build /app/pnpm-lock.yaml ./
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/node_modules ./node_modules

# Install production dependencies only
RUN pnpm install --prod --frozen-lockfile --force

# Expose port 3000
EXPOSE 3000

# Start the application
CMD ["pnpm", "start"]
