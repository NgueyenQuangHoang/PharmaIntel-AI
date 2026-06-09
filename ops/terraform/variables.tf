variable "aws_region" {
  description = "AWS region trien khai resources"
  type        = string
  default     = "ap-southeast-1"
}

variable "project_name" {
  description = "Ten du an, dung de tag resources"
  type        = string
  default     = "pharmaintel"
}

variable "environment" {
  description = "Moi truong (dev/staging/prod)"
  type        = string
  default     = "dev"
}

variable "vpc_cidr" {
  description = "CIDR block cho VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "public_subnet_cidr" {
  description = "CIDR block cho public subnet"
  type        = string
  default     = "10.0.1.0/24"
}

variable "instance_type" {
  description = "Loai EC2 instance"
  type        = string
  default     = "t2.micro"
}

variable "ami_id" {
  description = "AMI ID cho EC2 (Ubuntu 22.04)"
  type        = string
  default     = "ami-0c55b159cbfafe1f0"
}
