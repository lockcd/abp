import { NgClass, NgStyle } from '@angular/common';
import { ChangeDetectorRef, Component, Input, OnDestroy, OnInit, inject } from '@angular/core';
import { combineLatest, Subscription, timer } from 'rxjs';
import { HttpWaitService, RouterWaitService, SubscriptionService } from '@abp/ng.core';

@Component({
  selector: 'abp-loader-bar',
  template: `
    <div id="abp-loader-bar" [ngClass]="containerClass" [class.is-loading]="isLoading">
      <div
        class="abp-progress"
        [class.progressing]="progressLevel"
        [style.width.vw]="progressLevel"
        [ngStyle]="{
          'background-color': color,
          'box-shadow': boxShadow,
        }"
      ></div>
    </div>
  `,
  styleUrls: ['./loader-bar.component.scss'],
  providers: [SubscriptionService],
  imports: [NgClass, NgStyle],
})
export class LoaderBarComponent implements OnDestroy, OnInit {
  private cdRef = inject(ChangeDetectorRef);
  private subscription = inject(SubscriptionService);
  private httpWaitService = inject(HttpWaitService);
  private routerWaitService = inject(RouterWaitService);

  protected _isLoading!: boolean;

  @Input()
  set isLoading(value: boolean) {
    this._isLoading = value;
    this.cdRef.detectChanges();
  }
  get isLoading(): boolean {
    return this._isLoading;
  }

  @Input()
  containerClass = 'abp-loader-bar';

  @Input()
  color = '#77b6ff';

  progressLevel = 0;

  interval = new Subscription();

  timer = new Subscription();

  intervalPeriod = 350;

  stopDelay = 800;

  private readonly clearProgress = () => {
    this.progressLevel = 0;
    this.cdRef.detectChanges();
  };

  private readonly reportProgress = () => {
    if (this.progressLevel < 75) {
      this.progressLevel += 1 + Math.random() * 9;
    } else if (this.progressLevel < 90) {
      this.progressLevel += 0.4;
    } else if (this.progressLevel < 100) {
      this.progressLevel += 0.1;
    } else {
      this.interval.unsubscribe();
    }
    this.cdRef.detectChanges();
  };

  get boxShadow(): string {
    return `0 0 10px rgba(${this.color}, 0.5)`;
  }

  ngOnInit() {
    this.subscribeLoading();
  }

  subscribeLoading() {
    this.subscription.addOne(
      combineLatest([this.httpWaitService.getLoading$(), this.routerWaitService.getLoading$()]),
      ([httpLoading, routerLoading]) => {
        if (httpLoading || routerLoading) this.startLoading();
        else this.stopLoading();
      },
    );
  }

  ngOnDestroy() {
    this.interval.unsubscribe();
  }

  startLoading() {
    if (this.isLoading || !this.interval.closed) return;

    this.isLoading = true;
    this.progressLevel = 0;
    this.cdRef.detectChanges();
    this.interval = timer(0, this.intervalPeriod).subscribe(this.reportProgress);
    this.timer.unsubscribe();
  }

  stopLoading() {
    this.interval.unsubscribe();

    this.progressLevel = 100;
    this.isLoading = false;

    if (!this.timer.closed) return;

    this.timer = timer(this.stopDelay).subscribe(this.clearProgress);
  }
}
