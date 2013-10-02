#!/usr/bin/env python
#!encoding:utf-8
#!filename:zhihu_to_evernote.py
"""
Copyright 2013 zhihu_to_evernote
=======================================
author          = "Sam Huang"
name            = "zhihu_to_evernote"
version         = "1.0.1"
url             = "http://www.hiadmin.org"
author_email    = "sam.hxq@gmail.com"
=========================================

将知乎收藏问答页面的所有问答发送到Evernote/印象笔记中、
支持自己的知乎收藏或他人的知乎收藏。
因为Evernote/印象笔记可以直接将收到的附件转为笔记内容、
所以我直接将知乎问答通过html格式的邮件发送到Evernote中。

由于需要发送邮件，所以需要SMTP服务器以及发送邮件的账号和密码。
先将所需内容填写到config.ini配置中的对应位置即可
[info]
url = http://www.zhihu.com/collection/20261977
mail_host = smtp.126.com
mail_user = *********@126.com
mail_password = *******
evernote_mail=sam_hxq.58b4d9b@m.yinxiang.com 
notebook = 知乎收藏文章
"""
import os
import re
import requests
import configparser
import smtplib
from email.mime.text import MIMEText
from email.header import Header
from email.mime.multipart import MIMEMultipart
#from email.mime.image import MIMEImag



def collect_url(url):
    """
    接收用户输入的知乎收藏页面地址、
    然后提取知乎收藏页面的URL地址
    @url_list_regex       :提取URL规则、我们真正需要的是第二个()里面的内容
    @url_list             :用于存放从收藏页面提取出来的URL地址列表

    """      
    url_list_regex = r"(<h2 class.*href=\")(/\w+/\d+)(\">)(.*)(</a></h2>)"
    global url_list
    url_list = []
    list_temp = []
    r = requests.get(url)
    if r.status_code == 200:
        r_txt = r.text
        if re.search(url_list_regex,r_txt).group() != None:
            list_temp = re.findall(url_list_regex,r_txt)

    for i in list_temp:
        url_list.append("http://www.zhihu.com"+i[1])
    print (len(url_list))



def email_zhihu_content(url):
    """
    接收输入的单个收藏页面的URL地址、然后提取title、并将其作为邮件主题。
    用requests.get()下载页面内容、然后用smtplib发送html内容到
    Evernote的邮件地址中，Evernote会自动添加到笔记本中.
    @title_name_regex   :提取问答的title
    @subject            :用于作为邮件主题
    @

    """
    title_name_regex = r"(<title>)(.*)(\s-)(.*</title>)"
    #从配置文件读取内容
    cf = configparser.ConfigParser()
    cf.read("config.ini")
    mail_host = cf.get("info","mail_host")
    mail_user = cf.get("info","mail_user")
    mail_password = cf.get("info","mail_password")
    evernote_mail = cf.get("info","evernote_mail")
    notebook = "@" + cf.get("info","notebook")
    r = requests.get(url)
    if r.status_code == 200:
        html_txt = r.text
        title_name = re.search(title_name_regex,html_txt).group(2)
        print (title_name)
        subject = title_name

        #设置邮件主题
        msg = MIMEMultipart("alternative")
        msg["Subject"] = Header(subject + notebook,"utf-8")
        part = MIMEText(html_txt,"html") #设置以html格式发送内容
        msg.attach(part)
        
        #发送邮件
        smtp = smtplib.SMTP()
        smtp.connect(mail_host) 
        smtp.login(mail_user,mail_password) 
        smtp.sendmail(mail_user,evernote_mail,msg.as_string()) 
        smtp.quit() 
        



if __name__ == "__main__":
    cf = configparser.ConfigParser()
    cf.read("config.ini")
    url = cf.get("info","url")
    collect_url(url)
    for i in url_list:
        email_zhihu_content(i)
