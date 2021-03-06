import { CommentsService } from './../shared/comments.service';
import { Component, OnInit } from '@angular/core';
import { IComment } from '../blog-comments/IComment';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-blog-moderatorpanel',
  templateUrl: './blog-moderatorpanel.component.html',
  styleUrls: ['./blog-moderatorpanel.component.css'],
  animations: [
    trigger('reveal',[
      transition('void => *',[
        style({
          opacity : 0,
          transform : 'translateX(-30px)'
        }),
        animate(500)
      ])
    ])
  ]
})
export class BlogModeratorpanelComponent implements OnInit {

  unapprovedComments : IComment[];

  constructor(private commentService : CommentsService) { }

  ngOnInit() {
    this.commentService.getUnApprovedComments().subscribe(
      comments => this.unapprovedComments = comments
    )
  }

  onClickApprove($event){
    let comment = $event.target;
    let confirmation = confirm('Do you really want to approve this comment');
    if(confirmation){
      this.commentService.approveComment(comment.getAttribute('data-id')).subscribe(
        result => { alert("The comment has been approved."); comment.parentNode.parentNode.remove(); },
        error => alert(error.textStatus)
      );
    }
  }
  
  onClickDelete($event){
    let comment = $event.target;
    let confirmation = confirm('Do you really want to delete this comment');
    if(confirmation){
      this.commentService.deleteComment(comment.getAttribute('data-id')).subscribe(
        result => { alert("The comment has been deleted."); comment.parentNode.parentNode.remove(); },
        error => alert(error.textStatus)
      );
    }
  }

}
