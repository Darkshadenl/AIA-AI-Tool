FROM node:18 AS builder
WORKDIR /app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy rest of the application code
COPY . .

# Run build
RUN npm run build

CMD [ "npm", "run", "dev" ]