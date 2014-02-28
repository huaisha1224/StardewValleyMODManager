#!/usr/bin/env python
#!encoding:utf-8
#!filename:zhihu_to_evernote.py
"""
Copyright 2013 zhihu_to_evernote
=======================================
author              = "Sam Huang"
name                = "zhihu_to_evernote"
version             = "1.0.5"
url                 = "http://www.hiadmin.org"
author_email        = "sam.hxq@gmail.com"
Python_ver          = "Python3.x"
Requests_ver        = "Requests2.1.0"
BeautifulSoup_ver   = "BS4.2"
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
import sys
import requests
import configparser
import smtplib
from bs4 import BeautifulSoup
from email.mime.text import MIMEText
from email.header import Header
from email.mime.multipart import MIMEMultipart



#=============================================================================
def Next_page(url):
    """
    接收用户输入的知乎收藏页面地址、提取所有分页的url地址存入列表中
    通过正则表达式匹配内容并提取出来所有分页地址的url
    @url_list_regex :提取URL规则、如果为空说明本页已没收藏的文章
    @page_url       :用于存放所有分页url
    """
    url_list_regex = r"(<h2 class.*href=\")(/\w+/\d+)(\">)(.*)(</a></h2>)"
    global page_url
    page_url = []

    #通过知乎页面规则来增加后面的分页数量、
    #http://www.zhihu.com/collection/20261977?page=2 就表示第二页、以此类推。
    for i in range(1,1000):
        for x in url:
            url_temp = url + str(i)
        r = requests.get(url_temp)
        if r.status_code == 200:
            text_temp = r.text
        #用正则表达式去匹配是否有收藏内容，如果有则添加到列表中，否则循环中断
            if re.search(url_list_regex, text_temp) != None:
                page_url.append(url_temp)
            else:
                print ("No Next Page")
                break




#=============================================================================
def Collect_url(url):
    """
    接收用户输入的知乎收藏页面地址、
    然后提取知乎收藏页面的URL地址
    @url_list_regex         :提取URL规则、我们真正需要的是第二个()里面的内容
    @single_url_list        :用于存放从单个收藏页面提取出来的URL地址列表

    """

    url_list_regex = r"(<h2 class.*href=\")(/\w+/\d+)(\">)(.*)(</a></h2>)"
    global single_url_list
    single_url_list = []
    list_temp = []
    r = requests.get(url)
    if r.status_code == 200:
        r_txt = r.text
        #如果正则表达式匹配成功则说明里面有单个收藏链接、添加到列表中
        if re.search(url_list_regex,r_txt).group() != None:
            list_temp = re.findall(url_list_regex,r_txt)
        else:
            pass

    #用for循环吧列表里面的内容通过切片的时候提取出我们需要的，
    #并赋值给single_url_list
    for i in list_temp:
        single_url_list.append("http://www.zhihu.com"+i[1])

    for x in single_url_list:#将单页面url提取出来增加到url总表中
        url_list.append(x)


#=============================================================================
def Following_url(zhihu_user, zhihu_password):
    session = requests.session()
    login_res = session.post(
        'http://www.zhihu.com/login',
        data=dict(
            email=zhihu_user,
            password=zhihu_password
        )
    )
    login_res.raise_for_status()
    print('login done')
    xsrf = login_res.cookies['_xsrf']

    following_url_list = []
    result = _get_following_url(session, xsrf, 0)

    while len(result):
        following_url_list.extend(result)
        result = _get_following_url(session, xsrf, len(following_url_list))

    return following_url_list


def _get_following_url(session, xsrf, offset):
    print('fetching +{0}'.format(offset))
    res = session.post(
        'http://www.zhihu.com/node/ProfileFollowedQuestionsV2',
        data=dict(
            method='next',
            params='{"offset": %d}' % offset,
            _xsrf=xsrf
        )
    )
    try:
        result = res.json()
    except BaseException:
        import pdb; pdb.set_trace()
    if result['msg']:
        return [
            BeautifulSoup(msg).find(class_='question_link')['href']
            for msg in result['msg']
        ]
    return []


#=============================================================================
def Email_zhihu_content(url):
    """
    接收输入的单个收藏页面的URL地址、然后提取title、并将其作为邮件主题。
    用requests.get()下载页面内容、然后用smtplib发送html内容到
    Evernote的邮件地址中，Evernote会自动添加到笔记本中.
    @title_name_regex   :提取问答的title
    @subject            :用于作为邮件主题
    @

    """
    #提取问题的标题，并将其作为邮件的主题
    r = requests.get(url)
    if r.status_code == 200:
        html_txt = r.text
        soup = BeautifulSoup(html_txt)
        title_name = soup.title.string
        print (title_name)


        subject = title_name
        #设置邮件主题
        msg = MIMEMultipart("alternative")
        msg["Subject"] = Header(title_name + notebook,"utf-8")
        part = MIMEText(html_txt,"html") #设置以html格式发送内容
        msg.attach(part)

        #发送邮件
        smtp = smtplib.SMTP()
        smtp.connect(mail_host)
        smtp.login(mail_user,mail_password)
        smtp.sendmail(mail_user,evernote_mail,msg.as_string())
        smtp.quit()


#=============================================================================
if __name__ == "__main__":
    global url_list
    url_list = []
    #从配置文件读取内容
    cf = configparser.ConfigParser()
    cf.read("config.ini")
    url = cf.get("info","url") + "?page="
    mail_host = cf.get("info","mail_host")
    mail_user = cf.get("info","mail_user")
    mail_password = cf.get("info","mail_password")
    evernote_mail = cf.get("info","evernote_mail")
    notebook = "@" + cf.get("info","notebook")

    Next_page(url)

    for i in page_url: #循环所有收藏文章分页列表
        print (i)
        Collect_url(i)
    print (len(url_list))
    for x in url_list: #循环所有的单页收藏文章url列表
        print (x)
        Email_zhihu_content(x)
