﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.ViewModels.Input;
using DataAccess;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    public class PollsController : Controller
    {
        private UserManager<User> userManager;
        private IUnitOfWork unitOfWork;
        public PollsController(UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
        }

        //Get post polls
        [HttpGet("post/{postId}")]
        public IActionResult Get(long postId, string searchQuery, short numberOfItems = 10, short pageNumber = 1)
        {
            var polls = this.unitOfWork.PollsFetcher.GetPollsByPostId(postId,searchQuery,numberOfItems,pageNumber);
            return Ok(polls);
        }

        //Get user polls
        [Authorize(Roles = "Blogger")]
        [HttpGet("user")]
        public IActionResult GetUserPolls(string searchQuery, short numberOfItems = 10, short pageNumber = 1)
        {
            string userId = this.userManager.GetUserId(User);
            return Ok(this.unitOfWork.PollsFetcher.GetUserPolls(userId, searchQuery, numberOfItems, pageNumber));
        }

        // Get poll by pollId
        [HttpGet("{id}")]
        [ActionName("GetPoll")]
        public IActionResult GetById(int id)
        {
            var poll = this.unitOfWork.PollsFetcher.GetPollById(id);
            if(poll == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(poll);
            }
        }

        // Get poll results
        [HttpGet("results/{id}")]
        public IActionResult GetPollResults(int id)
        {
            return Ok(this.unitOfWork.PollsFetcher.GetPollResults(id));
        }

        [Authorize(Roles = "Blogger")]
        [HttpPost("attachpoll")]
        public async Task<IActionResult> AttachExistingPollToPost([FromBody]PollPostViewModel model)
        {
            var poll = this.unitOfWork.PollsRepository.GetById(model.PollId);
            var post = this.unitOfWork.PostsRepository.GetById(model.PostId);
            if (post == null || poll == null)
                return BadRequest();
            var userId = this.userManager.GetUserId(User);
            if (poll.UserId != userId || post.UserId != userId)
                return Forbid();
            PostPoll postPoll = new PostPoll
            {
                Poll = poll,
                Post = post
            };
            this.unitOfWork.PollsRepository.AddPollToPost(postPoll);
            await this.unitOfWork.Save();
            return StatusCode(201);
        }

        //Create a new poll
        [Authorize(Roles = "Blogger")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]NewPollViewModel model)
        {
            var userId = this.userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                Poll newPoll = new Poll
                {
                    Question = model.Question,
                    Active = true,
                    ActiveUntil = model.ActiveUntil,
                    MultipleAnswers = model.MultipleAnswers,
                    UserId = userId
                };
                this.unitOfWork.PollsRepository.Add(newPoll);
                foreach(var answer in model.Answers)
                {
                    PollAnswer newAnswer = new PollAnswer
                    {
                        Name = answer,
                        PollId = newPoll.PollId,
                    };
                    this.unitOfWork.PollAnswersRepository.Add(newAnswer);
                }
                await this.unitOfWork.Save();
                return CreatedAtAction("GetPoll", new { id = newPoll.PollId });
            }
            else
                return BadRequest();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Blogger")]
        public async Task<IActionResult> Put(int id, [FromBody]UpdatePollViewModel model)
        {
            if (ModelState.IsValid)
            {
                Poll poll = this.unitOfWork.PollsRepository.GetById(id);
                if (poll == null)
                    return BadRequest();
                else
                {
                    poll.Active = model.Active;
                    poll.ActiveUntil = model.ActiveUntil;
                    poll.Question = model.Question;
                    this.unitOfWork.PollsRepository.Update(poll);
                    await this.unitOfWork.Save();
                    return NoContent();
                }
            }
            else
            {
                return UnprocessableEntity();
            }
        }

        
        [Authorize(Roles = "Blogger")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var poll = this.unitOfWork.PollsRepository.GetById(id);
            if (poll == null)
                return BadRequest();
            else
            {
                var userId = this.userManager.GetUserId(User);
                if (userId != poll.UserId)
                    return Forbid();
                else
                {
                    this.unitOfWork.PollsRepository.Delete(poll);
                    await this.unitOfWork.Save();
                    return NoContent();
                }
            }
        }

        [Route("remove")]
        [Authorize(Roles = "Blogger")]
        [HttpDelete]
        public IActionResult RemovePollFromPost([FromBody]RemovePollFromPostsViewModel model)
        {
            var post = this.unitOfWork.PostsRepository.GetById(model.PostId);
            if (post == null)
                return BadRequest();
            if (post.UserId != User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value)
                return Forbid();
            var postPoll = this.unitOfWork.PollsRepository.GetPostPollById(model.PostPollId);
            if (postPoll == null)
                return BadRequest();
            this.unitOfWork.PollsRepository.RemoveFromPost(postPoll);
            this.unitOfWork.Save();
            return NoContent();
        }
    }
}
