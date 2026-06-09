FROM ubuntu:22.04

# Cai SSH server va python (Ansible can python)
RUN apt-get update && \
    apt-get install -y openssh-server python3 sudo && \
    apt-get clean

# Tao user ansible voi sudo NOPASSWD
RUN useradd -m -s /bin/bash ansible && \
    echo 'ansible:ansible' | chpasswd && \
    echo 'ansible ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers && \
    mkdir -p /var/run/sshd

# Cau hinh SSH cho phep password auth
RUN sed -i 's/#PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config && \
    sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/' /etc/ssh/sshd_config

EXPOSE 22

CMD ["/usr/sbin/sshd", "-D"]
