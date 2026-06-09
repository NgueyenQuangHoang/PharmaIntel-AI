# =============================================================================
# VPC - Mang ao chung cho toan bo ha tang
# =============================================================================
resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = {
    Name        = "${var.project_name}-vpc"
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# =============================================================================
# Internet Gateway - Cho phep VPC ket noi Internet
# =============================================================================
resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id

  tags = {
    Name        = "${var.project_name}-igw"
    Project     = var.project_name
    Environment = var.environment
  }
}

# =============================================================================
# Public Subnet - Subnet co IP public
# =============================================================================
resource "aws_subnet" "public" {
  vpc_id                  = aws_vpc.main.id
  cidr_block              = var.public_subnet_cidr
  availability_zone       = "${var.aws_region}a"
  map_public_ip_on_launch = true

  tags = {
    Name        = "${var.project_name}-public-subnet"
    Project     = var.project_name
    Environment = var.environment
    Type        = "Public"
  }
}

# =============================================================================
# Route Table - Dinh tuyen traffic ra Internet
# =============================================================================
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }

  tags = {
    Name        = "${var.project_name}-public-rt"
    Project     = var.project_name
    Environment = var.environment
  }
}

resource "aws_route_table_association" "public" {
  subnet_id      = aws_subnet.public.id
  route_table_id = aws_route_table.public.id
}

# =============================================================================
# Security Group - Firewall rules cho EC2
# =============================================================================
resource "aws_security_group" "web" {
  name        = "${var.project_name}-web-sg"
  description = "Cho phep HTTP, HTTPS, SSH"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "All outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project_name}-web-sg"
    Project     = var.project_name
    Environment = var.environment
  }
}

# =============================================================================
# EC2 Instance - VPS chinh chay ung dung
# =============================================================================
resource "aws_instance" "web" {
  ami                    = var.ami_id
  instance_type          = var.instance_type
  subnet_id              = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.web.id]

  user_data = <<-EOF
    #!/bin/bash
    apt-get update -y
    apt-get install -y docker.io nginx
    systemctl enable docker nginx
    systemctl start docker nginx
    echo "<h1>PharmaIntel - Deployed by Terraform</h1>" > /var/www/html/index.html
  EOF

  tags = {
    Name        = "${var.project_name}-web-server"
    Project     = var.project_name
    Environment = var.environment
    Role        = "WebServer"
  }
}

# =============================================================================
# S3 Bucket - Luu tru backup
# =============================================================================
resource "aws_s3_bucket" "backup" {
  bucket = "${var.project_name}-${var.environment}-backup"

  tags = {
    Name        = "${var.project_name}-backup"
    Project     = var.project_name
    Environment = var.environment
    Purpose     = "Backup"
  }
}

resource "aws_s3_bucket_versioning" "backup" {
  bucket = aws_s3_bucket.backup.id
  versioning_configuration {
    status = "Enabled"
  }
}

# =============================================================================
# S3 Bucket cho static assets
# =============================================================================
resource "aws_s3_bucket" "assets" {
  bucket = "${var.project_name}-${var.environment}-assets"

  tags = {
    Name        = "${var.project_name}-assets"
    Project     = var.project_name
    Environment = var.environment
    Purpose     = "StaticAssets"
  }
}
