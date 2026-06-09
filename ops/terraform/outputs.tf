output "vpc_id" {
  description = "ID cua VPC vua tao"
  value       = aws_vpc.main.id
}

output "public_subnet_id" {
  description = "ID cua public subnet"
  value       = aws_subnet.public.id
}

output "ec2_instance_id" {
  description = "ID cua EC2 instance"
  value       = aws_instance.web.id
}

output "ec2_public_ip" {
  description = "IP public cua EC2"
  value       = aws_instance.web.public_ip
}

output "ec2_public_dns" {
  description = "DNS public cua EC2"
  value       = aws_instance.web.public_dns
}

output "security_group_id" {
  description = "ID cua security group"
  value       = aws_security_group.web.id
}

output "s3_backup_bucket" {
  description = "Ten S3 bucket dung de backup"
  value       = aws_s3_bucket.backup.bucket
}

output "s3_assets_bucket" {
  description = "Ten S3 bucket dung cho static assets"
  value       = aws_s3_bucket.assets.bucket
}

output "ssh_command" {
  description = "Lenh SSH de ket noi EC2"
  value       = "ssh ubuntu@${aws_instance.web.public_ip}"
}

output "web_url" {
  description = "URL truy cap web sau khi deploy"
  value       = "http://${aws_instance.web.public_ip}"
}
